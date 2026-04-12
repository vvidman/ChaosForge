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

public sealed class ProjectStatusChangedHandlerTests
{
    private readonly IMediator _mediator = Substitute.For<IMediator>();
    private readonly IAgentInstanceRepository _agentInstanceRepository = Substitute.For<IAgentInstanceRepository>();

    private ProjectStatusChangedHandler CreateHandler() =>
        new(_mediator, _agentInstanceRepository, NullLogger<ProjectStatusChangedHandler>.Instance);

    [Fact]
    public async Task Handle_WhenTransitionToArchitecturePhase_MarksBusinessAnalystFinishedAndLeavesArchitectUntouched()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var baInstance = new AgentInstance(projectId, AgentRole.BusinessAnalyst, "BA-1");
        var architectInstance = new AgentInstance(projectId, AgentRole.Architect, "Arch-1");
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.RequirementsPhase, ProjectStatus.ArchitecturePhase);
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { baInstance, architectInstance });
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == baInstance.Id),
            Arg.Any<CancellationToken>());
        await _mediator.DidNotReceive().Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == architectInstance.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTransitionToCompleted_MarksAllInstancesFinished()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var developer = new AgentInstance(projectId, AgentRole.Developer, "Dev-1");
        var tester = new AgentInstance(projectId, AgentRole.Tester, "Tester-1");
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.Development, ProjectStatus.Completed);
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance> { developer, tester });
        var handler = CreateHandler();

        // Act
        await handler.Handle(notification, CancellationToken.None);

        // Assert
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == developer.Id),
            Arg.Any<CancellationToken>());
        await _mediator.Received(1).Send(
            Arg.Is<MarkAgentFinishedCommand>(c => c.AgentInstanceId == tester.Id),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenNoAgentInstancesExist_CompletesWithoutError()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var notification = new ProjectStatusChangedEvent(projectId, ProjectStatus.RequirementsPhase, ProjectStatus.ArchitecturePhase);
        _agentInstanceRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<AgentInstance>());
        var handler = CreateHandler();

        // Act
        var act = async () => await handler.Handle(notification, CancellationToken.None);

        // Assert
        await act.Should().NotThrowAsync();
        await _mediator.DidNotReceive().Send(Arg.Any<IRequest<object>>(), Arg.Any<CancellationToken>());
    }
}
