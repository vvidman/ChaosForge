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
/// Background worker for the Technical Writer agent. Runs during <see cref="ProjectStatus.Development"/>.
/// Picks the first <see cref="WorkTaskStatus.InDocumentation"/> task for the project, generates
/// documentation via LLM, then marks the task as <see cref="WorkTaskStatus.Done"/>.
/// </summary>
internal sealed class TechnicalWriterWorker : AgentWorkerBase
{
    private const string SystemPrompt =
        """
        You are a Technical Writer. Your task is to write clear, user-facing documentation
        for the feature described below. Include an overview, usage examples, and any
        important notes. Output well-structured Markdown.
        """;

    public TechnicalWriterWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<TechnicalWriterWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.TechnicalWriter;

    protected override ProjectStatus ActivePhase => ProjectStatus.Development;

    protected override async Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var workTaskRepo = scope.ServiceProvider.GetRequiredService<IWorkTaskRepository>();
        var taskAttemptRepo = scope.ServiceProvider.GetRequiredService<ITaskAttemptRepository>();
        var llmSelector = scope.ServiceProvider.GetRequiredService<ILlmProviderSelector>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<TechnicalWriterWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: find first InDocumentation task for this project
        var allTasks = await workTaskRepo.GetByProjectIdAsync(projectId, ct);
        var task = allTasks.FirstOrDefault(t => t.Status == WorkTaskStatus.InDocumentation);

        if (task is null)
        {
            return;
        }

        // Step 2: mark agent as Working
        await mediator.Send(new StartAgentWorkCommand(instance.Id, task.Id), ct);

        // Step 3: fetch most recent rejected Documentation attempt (if any)
        var attempts = await taskAttemptRepo.GetByWorkTaskIdAsync(task.Id, ct);
        var priorRejected = attempts
            .Where(a => a.Type == AttemptType.Documentation && a.Result == AttemptResult.Rejected)
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
                "TechnicalWriterWorker: LLM call failed for task {WorkTaskId}. Releasing agent {AgentInstanceId}.",
                task.Id,
                instance.Id);

            await mediator.Send(new FinishAgentWorkCommand(instance.Id), ct);

            return;
        }

        // Step 6: persist attempt
        var createResult = await mediator.Send(
            new CreateTaskAttemptCommand(task.Id, instance.Id, AttemptType.Documentation),
            ct);

        var attemptId = createResult.Value;

        await mediator.Send(new CompleteTaskAttemptCommand(attemptId, llmOutput), ct);
        await mediator.Send(new ApproveTaskAttemptCommand(attemptId), ct);

        // Step 7: mark task as Done
        await mediator.Send(new CompleteWorkTaskCommand(task.Id), ct);

        // Step 8: release agent
        await mediator.Send(new FinishAgentWorkCommand(instance.Id), ct);
    }

    // Exposed for unit testing via InternalsVisibleTo — do not call from production code.
    internal Task InvokeExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
        => ExecuteWorkAsync(scope, instance, ct);
}
