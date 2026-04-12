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
using ChaosForge.Application.SRS.Commands;
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
/// Background worker for the Architect agent. Runs during <see cref="ProjectStatus.ArchitecturePhase"/>.
/// For each URS item it generates an SRS via LLM, then decomposes each SRS into WorkTasks via a
/// second LLM call that returns structured JSON. When complete, opens an
/// <see cref="RevisionGateType.AfterArchitect"/> gate for human review.
/// </summary>
internal sealed class ArchitectWorker : AgentWorkerBase
{
    private const string SrsSystemPrompt =
        """
        You are a Software Architect. Your task is to produce a Software Requirements Specification (SRS)
        from the given User Requirements Specification (URS). Add sufficient technical detail so that a
        developer can derive implementation tasks directly from the output. Output plain text only.
        """;

    private const string WorkTaskSystemPrompt =
        """
        You are a Software Architect decomposing a Software Requirements Specification (SRS) into
        developer work tasks. Respond ONLY with a valid JSON array. Each element must have exactly
        three fields: "title" (string), "description" (string), and "storyPoints" (integer >= 1).
        Do not include any explanation or text outside the JSON array.
        """;

    public ArchitectWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<ArchitectWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.Architect;

    protected override ProjectStatus ActivePhase => ProjectStatus.ArchitecturePhase;

    protected override async Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var useCaseRepo = scope.ServiceProvider.GetRequiredService<IUseCaseRepository>();
        var ursRepo = scope.ServiceProvider.GetRequiredService<IURSRepository>();
        var srsRepo = scope.ServiceProvider.GetRequiredService<ISRSRepository>();
        var revisionGateRepo = scope.ServiceProvider.GetRequiredService<IRevisionGateRepository>();
        var llmSelector = scope.ServiceProvider.GetRequiredService<ILlmProviderSelector>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ArchitectWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: skip if an open AfterArchitect gate already exists
        var openGate = await revisionGateRepo.GetOpenByProjectIdAsync(projectId, ct);
        if (openGate is not null && openGate.Type == RevisionGateType.Architecture)
        {
            return;
        }

        // Step 2: collect all URS items for the project via use cases
        var useCases = await useCaseRepo.GetByProjectIdAsync(projectId, ct);
        var ursList = new List<URS>();
        foreach (var useCase in useCases)
        {
            var ursItems = await ursRepo.GetByUseCaseIdAsync(useCase.Id, ct);
            ursList.AddRange(ursItems);
        }

        if (ursList.Count == 0)
        {
            logger.LogWarning(
                "ArchitectWorker: no URS items found for project {ProjectId}. Skipping cycle.",
                projectId);

            return;
        }

        // Step 3: check for a prior rejected AfterArchitect gate to inject context
        var allGates = await revisionGateRepo.GetByProjectIdAsync(projectId, ct);
        var priorRejected = allGates
            .Where(g => g.Type == RevisionGateType.Architecture
                     && g.Status == RevisionGateStatus.Resolved
                     && g.Action == RevisionGateAction.Reject)
            .MaxBy(g => g.ResolvedAt);

        var effectiveSrsSystemPrompt = priorRejected is not null
            ? BuildSystemPromptWithRejection(priorRejected.RejectionReason)
            : SrsSystemPrompt;

        // Step 4: mark agent as Working (Guid.Empty sentinel — no WorkTask at phase level)
        await mediator.Send(new StartAgentWorkCommand(instance.Id, Guid.Empty), ct);

        // Step 5 (Pass 1): generate SRS for each URS via LLM
        var llm = llmSelector.GetProviderForRole(Role);
        var summaryParts = new List<string>(ursList.Count);

        foreach (var urs in ursList)
        {
            var userPrompt = BuildSrsUserPrompt(urs);

            string srsText;

            try
            {
                srsText = await llm.CompleteAsync(effectiveSrsSystemPrompt, userPrompt, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "ArchitectWorker: LLM call failed for URS {URSId}. Blocking agent {AgentInstanceId}.",
                    urs.Id,
                    instance.Id);

                await mediator.Send(new BlockAgentCommand(instance.Id), ct);

                return;
            }

            await mediator.Send(new CreateSRSCommand(urs.Id, urs.Title, srsText), ct);
            summaryParts.Add($"## {urs.Title}\n{srsText}");
        }

        // Step 6 (Pass 2): decompose each newly created SRS into WorkTasks
        foreach (var urs in ursList)
        {
            var srsItems = await srsRepo.GetByURSIdAsync(urs.Id, ct);
            var srs = srsItems.MaxBy(s => s.CreatedAt);

            if (srs is null)
            {
                continue;
            }

            string rawJson;

            try
            {
                rawJson = await llm.CompleteAsync(
                    WorkTaskSystemPrompt,
                    $"{srs.Title}\n{srs.TechnicalDescription}",
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "ArchitectWorker: LLM call failed for SRS {SRSId}. Blocking agent {AgentInstanceId}.",
                    srs.Id,
                    instance.Id);

                await mediator.Send(new BlockAgentCommand(instance.Id), ct);

                return;
            }

            List<WorkTaskDto>? tasks;

            try
            {
                tasks = JsonSerializer.Deserialize<List<WorkTaskDto>>(
                    rawJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (JsonException ex)
            {
                logger.LogError(
                    ex,
                    "ArchitectWorker: JSON parse failed for SRS {SRSId}. Raw output: {RawOutput}",
                    srs.Id,
                    rawJson);

                continue;
            }

            if (tasks is null)
            {
                logger.LogError(
                    "ArchitectWorker: JSON deserialized to null for SRS {SRSId}. Raw output: {RawOutput}",
                    srs.Id,
                    rawJson);

                continue;
            }

            foreach (var task in tasks)
            {
                var storyPoints = task.StoryPoints > 0 ? task.StoryPoints : 1;
                await mediator.Send(new CreateWorkTaskCommand(srs.Id, task.Title, task.Description, storyPoints), ct);
            }
        }

        // Step 7: open the AfterArchitect revision gate with the combined SRS output
        var agentOutput = string.Join("\n\n", summaryParts);
        await mediator.Send(new OpenRevisionGateCommand(projectId, RevisionGateType.Architecture, agentOutput), ct);

        // Step 8: mark agent as Finished
        await mediator.Send(new MarkAgentFinishedCommand(instance.Id), ct);
    }

    // Exposed for unit testing via InternalsVisibleTo — do not call from production code.
    internal Task InvokeExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
        => ExecuteWorkAsync(scope, instance, ct);

    private static string BuildSrsUserPrompt(URS urs)
    {
        if (urs.HumanEditNote is not null)
        {
            return $"[Human Edit Note: {urs.HumanEditNote}]\n\n{urs.Title}\n{urs.Description}";
        }

        return $"{urs.Title}\n{urs.Description}";
    }

    private static string BuildSystemPromptWithRejection(string? rejectionReason)
    {
        var reason = rejectionReason ?? "(no rejection reason provided)";

        return SrsSystemPrompt + "\n\n" +
            $"""
            --- Previous Attempt (Rejected) ---
            Rejection Reason:
            {reason}
            --- End of Previous Attempt ---
            """;
    }

    private sealed record WorkTaskDto(string Title, string Description, int StoryPoints);
}
