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

using ChaosForge.Application.AgentInstances.Commands;
using ChaosForge.Application.Orchestration;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace ChaosForge.Application.Tests.Orchestration;

public sealed class AgentInstanceActivationHandlerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IAgentSlotRepository _agentSlotRepository = Substitute.For<IAgentSlotRepository>();
    private readonly IAgentInstanceRepository _agentInstanceRepository = Substitute.For<IAgentInstanceRepository>();

    private AgentInstanceActivationHandler CreateHandler() =>
        new(_mediator, _agentSlotRepository, _agentInstanceRepository,
            NullLogger<AgentInstanceActivationHandler>.Instance);

    [Fact]
    public async Task Handle_WhenTransitionToRequirementsPhaseWithNoExistingInstances_SendsOneCreateCommand()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var slot = new AgentSlot(projectId, AgentRole.BusinessAnalyst, 1);
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.Setup, ProjectStatus.RequirementsPhase);
        _agentSlotRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentSlot> { slot });
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance>());
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateAgentInstanceCommand>(c =>
                c.ProjectId == projectId && c.Role == AgentRole.BusinessAnalyst),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionToDevelopmentWithOneExistingIdleInstance_SendsOneCreateCommand()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var slot = new AgentSlot(projectId, AgentRole.Developer, 2);
        var existingInstance = new AgentInstance(projectId, AgentRole.Developer, "Developer-existing");
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.SprintPlanning, ProjectStatus.Development);
        _agentSlotRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentSlot> { slot });
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { existingInstance });
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateAgentInstanceCommand>(c =>
                c.ProjectId == projectId && c.Role == AgentRole.Developer),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionToDevelopmentAndSlotFullyFilled_SendsNoCreateCommands()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var slot = new AgentSlot(projectId, AgentRole.Developer, 1);
        var existingInstance = new AgentInstance(projectId, AgentRole.Developer, "Developer-existing");
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.SprintPlanning, ProjectStatus.Development);
        _agentSlotRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentSlot> { slot });
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { existingInstance });
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.DidNotReceive().Send(
            Arg.Any<CreateAgentInstanceCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenExistingInstancesAreFinished_CreatesNewInstancesToFillSlot()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var slot = new AgentSlot(projectId, AgentRole.Developer, 1);
        var finishedInstance = new AgentInstance(projectId, AgentRole.Developer, "Developer-finished");
        finishedInstance.MarkFinished();
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.SprintPlanning, ProjectStatus.Development);
        _agentSlotRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentSlot> { slot });
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { finishedInstance });
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<CreateAgentInstanceCommand>(c =>
                c.ProjectId == projectId && c.Role == AgentRole.Developer),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionToCompleted_SendsNoCreateCommands()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.Development, ProjectStatus.Completed);
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _agentSlotRepository.DidNotReceive().GetByProjectIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Any<CreateAgentInstanceCommand>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoSlotFoundForActiveRole_LogsWarningAndSendsNoCommand()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.Setup, ProjectStatus.RequirementsPhase);
        _agentSlotRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentSlot>());
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance>());
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _mediator.DidNotReceive().Send(
            Arg.Any<CreateAgentInstanceCommand>(),
            Arg.Any<CancellationToken>());
    }
}
