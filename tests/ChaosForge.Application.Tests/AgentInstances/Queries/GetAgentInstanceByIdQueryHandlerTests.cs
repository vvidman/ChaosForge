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

using ChaosForge.Application.AgentInstances.Queries;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.AgentInstances.Queries;

public sealed class GetAgentInstanceByIdQueryHandlerTests
{
    private readonly IAgentInstanceRepository _agentInstanceRepository = Substitute.For<IAgentInstanceRepository>();

    private GetAgentInstanceByIdQueryHandler CreateHandler() =>
        new(_agentInstanceRepository);

    [Fact]
    public async Task Handle_WhenAgentInstanceNotFound_ReturnsFailure()
    {
        // Arrange
        var instanceId = Guid.NewGuid();
        _agentInstanceRepository.GetByIdAsync(instanceId, Arg.Any<CancellationToken>())
            .Returns((AgentInstance?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetAgentInstanceByIdQuery(instanceId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Agent instance not found.");
    }

    [Fact]
    public async Task Handle_WhenAgentInstanceFound_MapsAllFieldsCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Developer, "Dev-01");

        _agentInstanceRepository.GetByIdAsync(instance.Id, Arg.Any<CancellationToken>())
            .Returns(instance);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetAgentInstanceByIdQuery(instance.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(instance.Id);
        dto.ProjectId.Should().Be(projectId);
        dto.Role.Should().Be(AgentRole.Developer);
        dto.PersonaName.Should().Be("Dev-01");
        dto.Status.Should().Be(AgentInstanceStatus.Idle);
        dto.CurrentTaskId.Should().BeNull();
        dto.CreatedAt.Should().Be(instance.CreatedAt);
    }

    [Fact]
    public async Task Handle_WhenAgentInstanceHasCurrentTask_MapsCurrentTaskIdCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var taskId = Guid.NewGuid();
        var instance = new AgentInstance(projectId, AgentRole.Developer, "Dev-01");
        instance.StartWork(taskId);

        _agentInstanceRepository.GetByIdAsync(instance.Id, Arg.Any<CancellationToken>())
            .Returns(instance);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetAgentInstanceByIdQuery(instance.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.CurrentTaskId.Should().Be(taskId);
    }
}
