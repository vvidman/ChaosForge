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
using ChaosForge.Application.Projects.Commands;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChaosForge.Application.Orchestration;

internal sealed class RevisionGateResolvedHandler(
    IMediator mediator,
    IRevisionGateRepository revisionGateRepository,
    IButterflyService butterflyService,
    ILogger<RevisionGateResolvedHandler> logger)
    : INotificationHandler<RevisionGateResolvedEvent>
{
    public async Task Handle(RevisionGateResolvedEvent notification, CancellationToken cancellationToken)
    {
        if (notification.Action is RevisionGateAction.Reject)
        {
            logger.LogInformation(
                "Revision gate {GateId} was rejected — agent will retry on next poll.",
                notification.RevisionGateId);

            return;
        }

        var gate = await revisionGateRepository.GetByIdAsync(notification.RevisionGateId, cancellationToken);

        if (gate is null)
        {
            logger.LogWarning(
                "Revision gate {GateId} not found after resolution event — skipping phase transition.",
                notification.RevisionGateId);

            return;
        }

        if (notification.Action is RevisionGateAction.EditAndAccept)
        {
            await butterflyService.PropagateAsync(gate.Id, cancellationToken);
        }

        var nextStatus = gate.Type switch
        {
            RevisionGateType.Requirements => ProjectStatus.ArchitecturePhase,
            RevisionGateType.Architecture => ProjectStatus.SprintPlanning,
            RevisionGateType.SprintPlanning => ProjectStatus.Development,
            _ => throw new InvalidOperationException($"No phase transition defined for gate type {gate.Type}."),
        };

        await mediator.Send(new TransitionProjectCommand(gate.ProjectId, nextStatus), cancellationToken);
    }
}
