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

using System.Text.Json;
using ChaosForge.Application.Abstractions;
using ChaosForge.Application.AgentInstances.Commands;
using ChaosForge.Application.RevisionGates.Commands;
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
/// Background worker for the Scrum Master agent. Runs during <see cref="ProjectStatus.SprintPlanning"/>.
/// Uses the LLM to prioritize and select backlog tasks into a sprint, then opens a
/// <see cref="RevisionGateType.SprintPlanning"/> gate for human review.
/// </summary>
internal sealed class ScrumMasterWorker : AgentWorkerBase
{
    private const string SystemPrompt =
        """
        You are a Scrum Master. Your task is to review the product backlog and select the most
        valuable tasks to include in the next sprint. Prioritize tasks by business value,
        dependencies, and estimated effort. Respond ONLY with valid JSON in this exact format:
        {"sprintTaskIds":["guid1","guid2",...]}
        Include only the task IDs you select for the sprint. Do not include any explanation or
        text outside the JSON object.
        """;

    public ScrumMasterWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<ScrumMasterWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.ScrumMaster;

    protected override ProjectStatus ActivePhase => ProjectStatus.SprintPlanning;

    protected override async Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var projectRepo = scope.ServiceProvider.GetRequiredService<IProjectRepository>();
        var useCaseRepo = scope.ServiceProvider.GetRequiredService<IUseCaseRepository>();
        var ursRepo = scope.ServiceProvider.GetRequiredService<IURSRepository>();
        var srsRepo = scope.ServiceProvider.GetRequiredService<ISRSRepository>();
        var workTaskRepo = scope.ServiceProvider.GetRequiredService<IWorkTaskRepository>();
        var revisionGateRepo = scope.ServiceProvider.GetRequiredService<IRevisionGateRepository>();
        var llmSelector = scope.ServiceProvider.GetRequiredService<ILlmProviderSelector>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ScrumMasterWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: skip if an open SprintPlanning gate already exists
        var openGate = await revisionGateRepo.GetOpenByProjectIdAsync(projectId, ct);
        if (openGate is not null && openGate.Type == RevisionGateType.SprintPlanning)
        {
            return;
        }

        // Step 2: collect Backlog WorkTasks for the project via UseCase → URS → SRS → WorkTask
        var useCases = await useCaseRepo.GetByProjectIdAsync(projectId, ct);
        var backlogTasks = new List<WorkTask>();

        foreach (var useCase in useCases)
        {
            var ursList = await ursRepo.GetByUseCaseIdAsync(useCase.Id, ct);
            foreach (var urs in ursList)
            {
                var srsList = await srsRepo.GetByURSIdAsync(urs.Id, ct);
                foreach (var srs in srsList)
                {
                    var tasks = await workTaskRepo.GetBySRSIdAsync(srs.Id, ct);
                    backlogTasks.AddRange(tasks.Where(t => t.Status == WorkTaskStatus.Backlog));
                }
            }
        }

        // Step 3: skip with warning if no tasks found
        if (backlogTasks.Count == 0)
        {
            logger.LogWarning(
                "ScrumMasterWorker: no Backlog WorkTasks found for project {ProjectId}. Skipping cycle.",
                projectId);

            return;
        }

        // Step 4: mark agent as Working (Guid.Empty sentinel — no WorkTask at phase level)
        await mediator.Send(new StartAgentWorkCommand(instance.Id, Guid.Empty), ct);

        // Step 5: generate sprint Guid for this planning cycle
        var sprintId = Guid.NewGuid();

        // Step 6: build LLM user prompt — serialize tasks as JSON; inject deadline if set
        var project = await projectRepo.GetByIdAsync(projectId, ct);
        var userPrompt = BuildUserPrompt(backlogTasks, project?.Deadline);

        // Step 7: call LLM
        var llm = llmSelector.GetProviderForRole(Role);
        string rawResponse;

        try
        {
            rawResponse = await llm.CompleteAsync(SystemPrompt, userPrompt, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "ScrumMasterWorker: LLM call failed. Blocking agent {AgentInstanceId}.",
                instance.Id);

            await mediator.Send(new BlockAgentCommand(instance.Id), ct);

            return;
        }

        // Step 8: parse LLM response; fail-safe to all task IDs if unparseable
        var backlogIdSet = backlogTasks.Select(t => t.Id).ToHashSet();
        List<Guid> selectedIds;

        try
        {
            var dto = JsonSerializer.Deserialize<SprintPlanDto>(
                rawResponse,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (dto?.SprintTaskIds is { Count: > 0 })
            {
                selectedIds = dto.SprintTaskIds
                    .Select(s => Guid.TryParse(s, out var g) ? (Guid?)g : null)
                    .OfType<Guid>()
                    .Where(backlogIdSet.Contains)
                    .ToList();
            }
            else
            {
                logger.LogWarning(
                    "ScrumMasterWorker: LLM returned empty or null sprintTaskIds. Assigning all tasks.");

                selectedIds = [.. backlogIdSet];
            }
        }
        catch (JsonException ex)
        {
            logger.LogWarning(
                ex,
                "ScrumMasterWorker: LLM response was not valid JSON. Assigning all tasks. Raw: {Raw}",
                rawResponse);

            selectedIds = [.. backlogIdSet];
        }

        // Step 9: assign selected tasks to the sprint
        var taskById = backlogTasks.ToDictionary(t => t.Id);
        var assignedTasks = new List<WorkTask>(selectedIds.Count);

        foreach (var taskId in selectedIds)
        {
            await mediator.Send(new AssignWorkTaskToSprintCommand(taskId, sprintId), ct);
            assignedTasks.Add(taskById[taskId]);
        }

        // Step 10: build sprint plan summary for the revision gate
        var agentOutput = BuildSprintSummary(sprintId, assignedTasks);

        // Step 11: open the SprintPlanning revision gate
        await mediator.Send(new OpenRevisionGateCommand(projectId, RevisionGateType.SprintPlanning, agentOutput), ct);

        // Step 12: mark agent as Finished
        await mediator.Send(new MarkAgentFinishedCommand(instance.Id), ct);
    }

    // Exposed for unit testing via InternalsVisibleTo — do not call from production code.
    internal Task InvokeExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
        => ExecuteWorkAsync(scope, instance, ct);

    private static string BuildUserPrompt(List<WorkTask> tasks, DateTime? deadline)
    {
        var taskArray = tasks.Select(t => new
        {
            id = t.Id.ToString(),
            title = t.Title,
            description = t.Description,
            storyPoints = t.StoryPoints,
        });

        var tasksJson = JsonSerializer.Serialize(taskArray);

        if (deadline is not null)
        {
            return $"Project deadline: {deadline.Value:yyyy-MM-dd}\n\nBacklog tasks:\n{tasksJson}";
        }

        return $"Backlog tasks:\n{tasksJson}";
    }

    private static string BuildSprintSummary(Guid sprintId, List<WorkTask> assignedTasks)
    {
        var lines = assignedTasks.Select(t => $"- [{t.StoryPoints} pts] {t.Title}");

        return $"Sprint: {sprintId}\n\n{string.Join("\n", lines)}";
    }

    private sealed record SprintPlanDto(List<string>? SprintTaskIds);
}
