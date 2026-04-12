/*
   Copyright 2026 Viktor Vidman (vvidman)

   Licensed under the Apache License, Version 2.0 (the "License");
   you may not use this file except in compliance with the License.
   You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

   Unless required by applicable law or agreed to in writing, software
   distributed under the License is distributed on an "AS IS" BASIS,
   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
   See the License for the specific language governing permissions and
   limitations under the License.
*/

using ChaosForge.Application.Abstractions;
using ChaosForge.Application.AgentInstances.Commands;
using ChaosForge.Application.TaskAttempts.Commands;
using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.Agents;

/// <summary>
/// Background worker for the Reviewer agent. Runs during <see cref="ProjectStatus.Development"/>.
/// Picks the first <see cref="WorkTaskStatus.InReview"/> task for the project and generates a code
/// review via LLM. In this version the reviewer always approves, advancing the task to
/// <see cref="WorkTaskStatus.InTesting"/>. Human rejection via the API remains available.
/// </summary>
/// <remarks>
/// Known simplification: automatic rejection based on LLM quality evaluation is deferred.
/// The always-approve path is the safe baseline for the current implementation.
/// </remarks>
internal sealed class ReviewerWorker : AgentWorkerBase
{
    private const string SystemPrompt =
        """
        You are a Code Reviewer. Review the implementation provided below for the given task.
        Evaluate correctness, completeness, and adherence to the requirements.
        Provide concise, actionable feedback. Output plain text only.
        """;

    public ReviewerWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<ReviewerWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.Reviewer;

    protected override ProjectStatus ActivePhase => ProjectStatus.Development;

    protected override async Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var useCaseRepo = scope.ServiceProvider.GetRequiredService<IUseCaseRepository>();
        var ursRepo = scope.ServiceProvider.GetRequiredService<IURSRepository>();
        var srsRepo = scope.ServiceProvider.GetRequiredService<ISRSRepository>();
        var workTaskRepo = scope.ServiceProvider.GetRequiredService<IWorkTaskRepository>();
        var taskAttemptRepo = scope.ServiceProvider.GetRequiredService<ITaskAttemptRepository>();
        var llmSelector = scope.ServiceProvider.GetRequiredService<ILlmProviderSelector>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ReviewerWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: find first InReview task for this project
        var task = await FindEligibleTaskAsync(projectId, useCaseRepo, ursRepo, srsRepo, workTaskRepo, ct);

        if (task is null)
        {
            return;
        }

        // Step 2: mark agent as Working
        await mediator.Send(new StartAgentWorkCommand(instance.Id, task.Id), ct);

        // Step 3: fetch most recent rejected Review attempt (if any)
        var attempts = await taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, ct);
        var priorRejected = attempts
            .Where(a => a.Type == AttemptType.Review && a.Result == AttemptResult.Rejected)
            .MaxBy(a => a.StartedAt);

        // Step 4: build prompt
        var userPrompt = $"{task.Title}\n\n{task.Description}";
        var finalPrompt = AgentPromptBuilder.BuildWithPriorAttempt(userPrompt, priorRejected);

        // Step 5: call LLM
        var llm = llmSelector.GetProviderForRole(Role);
        string llmOutput;

        try
        {
            llmOutput = await llm.CompleteAsync(SystemPrompt, finalPrompt, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "ReviewerWorker: LLM call failed for task {WorkTaskId}. Releasing agent {AgentInstanceId}.",
                task.Id,
                instance.Id);

            await mediator.Send(new FinishAgentWorkCommand(instance.Id), ct);

            return;
        }

        // Step 6: persist attempt
        var createResult = await mediator.Send(
            new CreateTaskAttemptCommand(task.Id, instance.Id, AttemptType.Review),
            ct);

        var attemptId = createResult.Value;

        await mediator.Send(new CompleteTaskAttemptCommand(attemptId, llmOutput), ct);

        // Step 7: always approve (rejection logic deferred — see class remarks)
        await mediator.Send(new ApproveTaskAttemptCommand(attemptId), ct);
        await mediator.Send(new ApproveWorkTaskCommand(task.Id), ct);

        // Step 8: release agent
        await mediator.Send(new FinishAgentWorkCommand(instance.Id), ct);
    }

    // Exposed for unit testing via InternalsVisibleTo — do not call from production code.
    internal Task InvokeExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
        => ExecuteWorkAsync(scope, instance, ct);

    private static async Task<WorkTask?> FindEligibleTaskAsync(
        Guid projectId,
        IUseCaseRepository useCaseRepo,
        IURSRepository ursRepo,
        ISRSRepository srsRepo,
        IWorkTaskRepository workTaskRepo,
        CancellationToken ct)
    {
        var useCases = await useCaseRepo.GetByProjectIdAsync(projectId, ct);

        foreach (var useCase in useCases)
        {
            var ursList = await ursRepo.GetByUseCaseIdAsync(useCase.Id, ct);

            foreach (var urs in ursList)
            {
                var srsList = await srsRepo.GetByURSIdAsync(urs.Id, ct);

                foreach (var srs in srsList)
                {
                    var tasks = await workTaskRepo.GetBySRSIdAsync(srs.Id, ct);
                    var eligible = tasks.FirstOrDefault(t => t.Status == WorkTaskStatus.InReview);

                    if (eligible is not null)
                    {
                        return eligible;
                    }
                }
            }
        }

        return null;
    }
}
