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

using ChaosForge.Application.Common;
using ChaosForge.Application.Orchestration;
using ChaosForge.Application.SRS.Commands;
using ChaosForge.Application.URS.Commands;
using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using SrsEntity = ChaosForge.Domain.Entities.SRS;
using UrsEntity = ChaosForge.Domain.Entities.URS;
using UseCase = ChaosForge.Domain.Entities.UseCase;
using WorkTask = ChaosForge.Domain.Entities.WorkTask;
using RevisionGate = ChaosForge.Domain.Entities.RevisionGate;

namespace ChaosForge.Application.Tests.Orchestration;

public sealed class ButterflyServiceTests
{
    private readonly IRevisionGateRepository _revisionGateRepository = Substitute.For<IRevisionGateRepository>();
    private readonly IUseCaseRepository _useCaseRepository = Substitute.For<IUseCaseRepository>();
    private readonly IURSRepository _ursRepository = Substitute.For<IURSRepository>();
    private readonly ISRSRepository _srsRepository = Substitute.For<ISRSRepository>();
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();
    private readonly IMediator _mediator = Substitute.For<IMediator>();

    private ButterflyService CreateService() =>
        new(
            _revisionGateRepository,
            _useCaseRepository,
            _ursRepository,
            _srsRepository,
            _workTaskRepository,
            _mediator,
            NullLogger<ButterflyService>.Instance);

    // ── AfterBA (Requirements gate) ──────────────────────────────────────────

    [Fact]
    public async Task PropagateAsync_AfterBA_SendsApplyHumanEditToURSForEachURS()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Requirements, "Agent output.");
        gate.EditAndAccept("Human edited URS.");

        var useCase = new UseCase(projectId, "UC1", "Description", 0);
        var urs1 = new UrsEntity(useCase.Id, "URS 1", "Req 1");
        var urs2 = new UrsEntity(useCase.Id, "URS 2", "Req 2");

        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        _useCaseRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase });
        _ursRepository.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<UrsEntity> { urs1, urs2 });
        _mediator.Send(Arg.Any<ApplyHumanEditToURSCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var service = CreateService();

        // Act
        await service.PropagateAsync(gate.Id, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ApplyHumanEditToURSCommand>(c => c.URSId == urs1.Id),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<ApplyHumanEditToURSCommand>(c => c.URSId == urs2.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PropagateAsync_AfterBA_WhenNoURSItems_NoCommandsSentAndNoException()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Requirements, "Agent output.");
        gate.EditAndAccept("Human edited URS.");

        var useCase = new UseCase(projectId, "UC1", "Description", 0);

        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        _useCaseRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase });
        _ursRepository.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<UrsEntity>());

        var service = CreateService();

        // Act
        var act = async () => await service.PropagateAsync(gate.Id, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _mediator.DidNotReceive().Send(Arg.Any<ApplyHumanEditToURSCommand>(), Arg.Any<CancellationToken>());
    }

    // ── AfterArchitect (Architecture gate) ───────────────────────────────────

    [Fact]
    public async Task PropagateAsync_AfterArchitect_SendsApplyHumanEditToSRSAndDeletesBacklogTasksOnly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Architecture, "Architect output.");
        gate.EditAndAccept("Human edited SRS.");

        var useCase = new UseCase(projectId, "UC1", "Description", 0);
        var urs = new UrsEntity(useCase.Id, "URS 1", "Req 1");
        var srs = new SrsEntity(urs.Id, "SRS 1", "Tech desc 1");

        var backlogTask = new WorkTask(srs.Id, "Task A", "Do it", 2);
        var inProgressTask = new WorkTask(srs.Id, "Task B", "In flight", 3);
        inProgressTask.AssignToSprint(Guid.NewGuid());
        inProgressTask.Start();

        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        _useCaseRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase });
        _ursRepository.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<UrsEntity> { urs });
        _srsRepository.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SrsEntity> { srs });
        _workTaskRepository.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { backlogTask, inProgressTask });
        _mediator.Send(Arg.Any<ApplyHumanEditToSRSCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());
        _mediator.Send(Arg.Any<DeleteWorkTaskCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var service = CreateService();

        // Act
        await service.PropagateAsync(gate.Id, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ApplyHumanEditToSRSCommand>(c => c.SRSId == srs.Id),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<DeleteWorkTaskCommand>(c => c.WorkTaskId == backlogTask.Id),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Is<DeleteWorkTaskCommand>(c => c.WorkTaskId == inProgressTask.Id),
            Arg.Any<CancellationToken>());
    }

    // ── AfterScrumMaster (SprintPlanning gate) ────────────────────────────────

    [Fact]
    public async Task PropagateAsync_AfterScrumMaster_SendsClearSprintForAllAssignedTasks()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.SprintPlanning, "SM output.");
        gate.EditAndAccept("Human edited sprint plan.");

        var useCase = new UseCase(projectId, "UC1", "Description", 0);
        var urs = new UrsEntity(useCase.Id, "URS 1", "Req 1");
        var srs = new SrsEntity(urs.Id, "SRS 1", "Tech desc 1");

        var assignedTask = new WorkTask(srs.Id, "Assigned Task", "Do it", 2);
        assignedTask.AssignToSprint(Guid.NewGuid());
        var unassignedTask = new WorkTask(srs.Id, "Unassigned Task", "Not started", 1);

        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        _useCaseRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase });
        _ursRepository.GetByUseCaseIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(new List<UrsEntity> { urs });
        _srsRepository.GetByURSIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<SrsEntity> { srs });
        _workTaskRepository.GetBySRSIdAsync(srs.Id, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { assignedTask, unassignedTask });
        _mediator.Send(Arg.Any<ClearWorkTaskSprintCommand>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success());

        var service = CreateService();

        // Act
        await service.PropagateAsync(gate.Id, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<ClearWorkTaskSprintCommand>(c => c.WorkTaskId == assignedTask.Id),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Is<ClearWorkTaskSprintCommand>(c => c.WorkTaskId == unassignedTask.Id),
            Arg.Any<CancellationToken>());
    }
}
