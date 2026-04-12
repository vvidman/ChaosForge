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
/// Background worker for the Tester agent. Runs during <see cref="ProjectStatus.Development"/>.
/// Picks the first <see cref="WorkTaskStatus.InTesting"/> task for the project, generates test
/// cases via LLM, then always passes the task to <see cref="WorkTaskStatus.InDocumentation"/>.
/// Human rejection via the API remains available.
/// </summary>
/// <remarks>
/// Known simplification: automatic rejection based on LLM quality evaluation is deferred.
/// The always-pass path is the safe baseline for the current implementation.
/// </remarks>
internal sealed class TesterWorker : AgentWorkerBase
{
    private const string SystemPrompt =
        """
        You are a Software Tester. Your task is to write test cases for the work item described below.
        Cover the main success path, edge cases, and error conditions.
        Output plain text with a structured list of test cases.
        """;

    public TesterWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<TesterWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.Tester;

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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TesterWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: find first InTesting task for this project
        var task = await FindEligibleTaskAsync(projectId, useCaseRepo, ursRepo, srsRepo, workTaskRepo, ct);

        if (task is null)
        {
            return;
        }

        // Step 2: mark agent as Working
        await mediator.Send(new StartAgentWorkCommand(instance.Id, task.Id), ct);

        // Step 3: fetch most recent rejected Testing attempt (if any)
        var attempts = await taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, ct);
        var priorRejected = attempts
            .Where(a => a.Type == AttemptType.Testing && a.Result == AttemptResult.Rejected)
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
                "TesterWorker: LLM call failed for task {WorkTaskId}. Releasing agent {AgentInstanceId}.",
                task.Id,
                instance.Id);

            await mediator.Send(new FinishAgentWorkCommand(instance.Id), ct);

            return;
        }

        // Step 6: persist attempt
        var createResult = await mediator.Send(
            new CreateTaskAttemptCommand(task.Id, instance.Id, AttemptType.Testing),
            ct);

        var attemptId = createResult.Value;

        await mediator.Send(new CompleteTaskAttemptCommand(attemptId, llmOutput), ct);

        // Step 7: always pass (rejection logic deferred — see class remarks)
        await mediator.Send(new ApproveTaskAttemptCommand(attemptId), ct);
        await mediator.Send(new PassWorkTaskTestingCommand(task.Id), ct);

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
                    var eligible = tasks.FirstOrDefault(t => t.Status == WorkTaskStatus.InTesting);

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
