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
using ChaosForge.Application.Orchestration;
using ChaosForge.Application.Projects.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace ChaosForge.Application.Tests.Orchestration;

public sealed class RevisionGateResolvedHandlerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IRevisionGateRepository _revisionGateRepository = Substitute.For<IRevisionGateRepository>();
    private readonly IButterflyService _butterflyService = Substitute.For<IButterflyService>();

    private RevisionGateResolvedHandler CreateHandler() =>
        new(_mediator, _revisionGateRepository, _butterflyService, NullLogger<RevisionGateResolvedHandler>.Instance);

    [Fact]
    public async Task Handle_WhenActionIsReject_NoCommandSentAndNoButterflyCall()
    {
        // Arrange
        var notification = new RevisionGateResolvedEvent(Guid.NewGuid(), Guid.NewGuid(), RevisionGateAction.Reject);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.DidNotReceive().Send(Arg.Any<IRequest<object>>(), Arg.Any<CancellationToken>());
        await _butterflyService.DidNotReceive().PropagateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAcceptOnRequirementsGate_SendsTransitionToArchitecturePhaseWithoutButterflyCall()
    {
        // Arrange
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "BA output.");
        gate.Accept();
        var notification = new RevisionGateResolvedEvent(gate.Id, gate.ProjectId, RevisionGateAction.Accept);
        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<TransitionProjectCommand>(c =>
                c.ProjectId == gate.ProjectId && c.NewStatus == ProjectStatus.ArchitecturePhase),
            Arg.Any<CancellationToken>());
        await _butterflyService.DidNotReceive().PropagateAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEditAndAcceptOnArchitectureGate_CallsButterflyFirstThenTransitionToSprintPlanning()
    {
        // Arrange
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Architecture, "Architect output.");
        gate.EditAndAccept("Edited architect output.");
        var notification = new RevisionGateResolvedEvent(gate.Id, gate.ProjectId, RevisionGateAction.EditAndAccept);
        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _butterflyService.Received(1).PropagateAsync(gate.Id, Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<TransitionProjectCommand>(c =>
                c.ProjectId == gate.ProjectId && c.NewStatus == ProjectStatus.SprintPlanning),
            Arg.Any<CancellationToken>());
    }
}
