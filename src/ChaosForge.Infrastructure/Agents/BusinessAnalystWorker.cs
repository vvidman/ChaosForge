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
using ChaosForge.Application.RevisionGates.Commands;
using ChaosForge.Application.URS.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace ChaosForge.Infrastructure.Agents;

/// <summary>
/// Background worker for the Business Analyst agent. Runs during <see cref="ProjectStatus.RequirementsPhase"/>,
/// generates a URS for every UseCase in the project via the LLM, persists the results,
/// then opens an <see cref="RevisionGateType.Requirements"/> gate for human review.
/// </summary>
internal sealed class BusinessAnalystWorker : AgentWorkerBase
{
    private const string SystemPrompt =
        """
        You are a Business Analyst. Your task is to produce a User Requirements Specification (URS)
        for the given use case. Write clearly and precisely. Focus on what the system must do,
        not how it does it. Output plain text only.
        """;

    public BusinessAnalystWorker(
        IServiceScopeFactory scopeFactory,
        IOptions<AgentWorkerOptions> options,
        ILogger<BusinessAnalystWorker> logger)
        : base(scopeFactory, options, logger)
    {
    }

    protected override AgentRole Role => AgentRole.BusinessAnalyst;

    protected override ProjectStatus ActivePhase => ProjectStatus.RequirementsPhase;

    protected override async Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
    {
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var useCaseRepo = scope.ServiceProvider.GetRequiredService<IUseCaseRepository>();
        var revisionGateRepo = scope.ServiceProvider.GetRequiredService<IRevisionGateRepository>();
        var llmSelector = scope.ServiceProvider.GetRequiredService<ILlmProviderSelector>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<BusinessAnalystWorker>>();

        var projectId = instance.ProjectId;

        // Step 1: skip if an open Requirements gate already exists
        var openGate = await revisionGateRepo.GetOpenByProjectIdAsync(projectId, ct);
        if (openGate is not null && openGate.Type == RevisionGateType.Requirements)
        {
            return;
        }

        // Step 2: fetch all use cases; skip with warning if none
        var useCases = await useCaseRepo.GetByProjectIdAsync(projectId, ct);
        if (useCases.Count == 0)
        {
            logger.LogWarning(
                "BusinessAnalystWorker: no UseCases found for project {ProjectId}. Skipping cycle.",
                projectId);

            return;
        }

        // Step 3: check for prior rejected Requirements gate to inject context
        var allGates = await revisionGateRepo.GetByProjectIdAsync(projectId, ct);
        var priorRejected = allGates
            .Where(g => g.Type == RevisionGateType.Requirements
                     && g.Status == RevisionGateStatus.Resolved
                     && g.Action == RevisionGateAction.Reject)
            .MaxBy(g => g.ResolvedAt);

        var effectiveSystemPrompt = priorRejected is not null
            ? BuildSystemPromptWithRejection(priorRejected.RejectionReason)
            : SystemPrompt;

        // Step 4: mark agent as Working (Guid.Empty sentinel — no WorkTask at phase level)
        await mediator.Send(new StartAgentWorkCommand(instance.Id, Guid.Empty), ct);

        // Step 5: call LLM for each UseCase and create a URS; block on any LLM failure
        var llm = llmSelector.GetProviderForRole(Role);
        var summaryParts = new List<string>(useCases.Count);

        foreach (var useCase in useCases)
        {
            string ursText;

            try
            {
                ursText = await llm.CompleteAsync(
                    effectiveSystemPrompt,
                    useCase.Title + "\n" + useCase.Description,
                    ct);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "BusinessAnalystWorker: LLM call failed for UseCase {UseCaseId}. Blocking agent {AgentInstanceId}.",
                    useCase.Id,
                    instance.Id);

                await mediator.Send(new BlockAgentCommand(instance.Id), ct);

                return;
            }

            await mediator.Send(new CreateURSCommand(useCase.Id, useCase.Title, ursText), ct);
            summaryParts.Add($"## {useCase.Title}\n{ursText}");
        }

        // Step 6: open the Requirements revision gate with the combined URS output
        var agentOutput = string.Join("\n\n", summaryParts);
        await mediator.Send(new OpenRevisionGateCommand(projectId, RevisionGateType.Requirements, agentOutput), ct);

        // Step 7: mark agent as Finished
        await mediator.Send(new MarkAgentFinishedCommand(instance.Id), ct);
    }

    // Exposed for unit testing via InternalsVisibleTo — do not call from production code.
    internal Task InvokeExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)
        => ExecuteWorkAsync(scope, instance, ct);

    private static string BuildSystemPromptWithRejection(string? rejectionReason)
    {
        var reason = rejectionReason ?? "(no rejection reason provided)";

        return SystemPrompt + "\n\n" +
            $"""
            --- Previous Attempt (Rejected) ---
            Rejection Reason:
            {reason}
            --- End of Previous Attempt ---
            """;
    }
}
