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
using ChaosForge.Application.SRS.Commands;
using ChaosForge.Application.URS.Commands;
using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChaosForge.Application.Orchestration;

internal sealed class ButterflyService(
    IRevisionGateRepository revisionGateRepository,
    IUseCaseRepository useCaseRepository,
    IURSRepository ursRepository,
    ISRSRepository srsRepository,
    IWorkTaskRepository workTaskRepository,
    IMediator mediator,
    ILogger<ButterflyService> logger)
    : IButterflyService
{
    public async Task PropagateAsync(Guid revisionGateId, CancellationToken cancellationToken = default)
    {
        var gate = await revisionGateRepository.GetByIdAsync(revisionGateId, cancellationToken);

        if (gate is null)
        {
            logger.LogWarning(
                "ButterflyService: revision gate {GateId} not found — skipping propagation.",
                revisionGateId);

            return;
        }

        if (gate.HumanEditedOutput is null)
        {
            logger.LogWarning(
                "ButterflyService: gate {GateId} has no HumanEditedOutput — skipping propagation.",
                revisionGateId);

            return;
        }

        switch (gate.Type)
        {
            case RevisionGateType.Requirements:
                await PropagateAfterBAAsync(gate.ProjectId, gate.HumanEditedOutput, cancellationToken);
                break;

            case RevisionGateType.Architecture:
                await PropagateAfterArchitectAsync(gate.ProjectId, gate.HumanEditedOutput, cancellationToken);
                break;

            case RevisionGateType.SprintPlanning:
                await PropagateAfterScrumMasterAsync(gate.ProjectId, cancellationToken);
                break;

            default:
                logger.LogWarning(
                    "ButterflyService: unhandled gate type {GateType} for gate {GateId}.",
                    gate.Type,
                    revisionGateId);
                break;
        }
    }

    private async Task PropagateAfterBAAsync(
        Guid projectId,
        string humanEditedOutput,
        CancellationToken cancellationToken)
    {
        var useCases = await useCaseRepository.GetByProjectIdAsync(projectId, cancellationToken);

        foreach (var useCase in useCases)
        {
            var ursItems = await ursRepository.GetByUseCaseIdAsync(useCase.Id, cancellationToken);

            foreach (var urs in ursItems)
            {
                var result = await mediator.Send(
                    new ApplyHumanEditToURSCommand(urs.Id, humanEditedOutput, "Human edit applied via ButterflyService."),
                    cancellationToken);

                if (!result.IsSuccess)
                {
                    logger.LogWarning(
                        "ButterflyService: failed to apply human edit to URS {URSId}: {Error}",
                        urs.Id,
                        result.Error);
                }
            }
        }
    }

    private async Task PropagateAfterArchitectAsync(
        Guid projectId,
        string humanEditedOutput,
        CancellationToken cancellationToken)
    {
        var useCases = await useCaseRepository.GetByProjectIdAsync(projectId, cancellationToken);

        foreach (var useCase in useCases)
        {
            var ursItems = await ursRepository.GetByUseCaseIdAsync(useCase.Id, cancellationToken);

            foreach (var urs in ursItems)
            {
                var srsItems = await srsRepository.GetByURSIdAsync(urs.Id, cancellationToken);

                foreach (var srs in srsItems)
                {
                    var srsResult = await mediator.Send(
                        new ApplyHumanEditToSRSCommand(srs.Id, humanEditedOutput, "Human edit applied via ButterflyService."),
                        cancellationToken);

                    if (!srsResult.IsSuccess)
                    {
                        logger.LogWarning(
                            "ButterflyService: failed to apply human edit to SRS {SRSId}: {Error}",
                            srs.Id,
                            srsResult.Error);
                    }

                    var tasks = await workTaskRepository.GetBySRSIdAsync(srs.Id, cancellationToken);

                    foreach (var task in tasks.Where(t => t.Status == WorkTaskStatus.Backlog))
                    {
                        var deleteResult = await mediator.Send(
                            new DeleteWorkTaskCommand(task.Id),
                            cancellationToken);

                        if (!deleteResult.IsSuccess)
                        {
                            logger.LogWarning(
                                "ButterflyService: failed to delete Backlog WorkTask {TaskId}: {Error}",
                                task.Id,
                                deleteResult.Error);
                        }
                    }
                }
            }
        }
    }

    private async Task PropagateAfterScrumMasterAsync(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var useCases = await useCaseRepository.GetByProjectIdAsync(projectId, cancellationToken);

        foreach (var useCase in useCases)
        {
            var ursItems = await ursRepository.GetByUseCaseIdAsync(useCase.Id, cancellationToken);

            foreach (var urs in ursItems)
            {
                var srsItems = await srsRepository.GetByURSIdAsync(urs.Id, cancellationToken);

                foreach (var srs in srsItems)
                {
                    var tasks = await workTaskRepository.GetBySRSIdAsync(srs.Id, cancellationToken);

                    foreach (var task in tasks.Where(t => t.SprintId is not null))
                    {
                        var result = await mediator.Send(
                            new ClearWorkTaskSprintCommand(task.Id),
                            cancellationToken);

                        if (!result.IsSuccess)
                        {
                            logger.LogWarning(
                                "ButterflyService: failed to clear sprint for WorkTask {TaskId}: {Error}",
                                task.Id,
                                result.Error);
                        }
                    }
                }
            }
        }
    }
}
