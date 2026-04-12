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
/// Background worker for the Developer agent. Runs during <see cref="ProjectStatus.Development"/>.
/// Picks the first <see cref="WorkTaskStatus.Backlog"/> task with a sprint assigned, generates an
/// implementation via LLM, then advances the task through
/// <see cref="WorkTaskStatus.InProgress"/> → <see cref="WorkTaskStatus.InReview"/>.
/// </summary>
internal sealed class DeveloperWorker : AgentWorkerBase
{
    private const string SystemPrompt =
        """
        You are a Software Developer. Your task is to implement the work item described below.
        Write a complete implementation using Markdown with fenced code blocks.
        Cover the full scope of the description — do not leave placeholders or TODOs.
        """;

    public DeveloperWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<DeveloperWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.Developer;

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
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DeveloperWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: find first Backlog task with SprintId set for this project
        var task = await FindEligibleTaskAsync(projectId, useCaseRepo, ursRepo, srsRepo, workTaskRepo, ct);

        if (task is null)
        {
            return;
        }

        // Step 2: mark agent as Working
        await mediator.Send(new StartAgentWorkCommand(instance.Id, task.Id), ct);

        // Step 3: fetch most recent rejected Review attempt for this task (if any) — the
        // developer uses review feedback to improve the next implementation attempt.
        var attempts = await taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, ct);
        var priorRejected = attempts
            .Where(a => a.Type == AttemptType.Review && a.Result == AttemptResult.Rejected)
            .MaxBy(a => a.StartedAt);

        // Step 4: build user prompt, injecting prior rejection context when available
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
                "DeveloperWorker: LLM call failed for task {WorkTaskId}. Releasing agent {AgentInstanceId}.",
                task.Id,
                instance.Id);

            await mediator.Send(new FinishAgentWorkCommand(instance.Id), ct);

            return;
        }

        // Step 6: persist attempt
        var createResult = await mediator.Send(
            new CreateTaskAttemptCommand(task.Id, instance.Id, AttemptType.Implementation),
            ct);

        var attemptId = createResult.Value;

        await mediator.Send(new CompleteTaskAttemptCommand(attemptId, llmOutput), ct);

        // Step 7: advance task status Backlog → InProgress → InReview
        await mediator.Send(new StartWorkTaskCommand(task.Id), ct);
        await mediator.Send(new SendWorkTaskToReviewCommand(task.Id), ct);

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
                    var eligible = tasks.FirstOrDefault(
                        t => t.Status == WorkTaskStatus.Backlog && t.SprintId is not null);

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
