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

using ChaosForge.Application.TaskAttempts.Queries;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.TaskAttempts.Queries;

public sealed class GetTaskAttemptByIdQueryHandlerTests
{
    private readonly ITaskAttemptRepository _taskAttemptRepository = Substitute.For<ITaskAttemptRepository>();

    private GetTaskAttemptByIdQueryHandler CreateHandler() =>
        new(_taskAttemptRepository);

    [Fact]
    public async Task Handle_WhenTaskAttemptNotFound_ReturnsFailure()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        _taskAttemptRepository.GetByIdAsync(attemptId, Arg.Any<CancellationToken>())
            .Returns((TaskAttempt?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetTaskAttemptByIdQuery(attemptId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Task attempt not found.");
    }

    [Fact]
    public async Task Handle_WhenTaskAttemptFound_MapsNullableFieldsAsNull()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        var agentInstanceId = Guid.NewGuid();
        var attempt = new TaskAttempt(workTaskId, agentInstanceId, AttemptType.Implementation);

        _taskAttemptRepository.GetByIdAsync(attempt.Id, Arg.Any<CancellationToken>())
            .Returns(attempt);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetTaskAttemptByIdQuery(attempt.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(attempt.Id);
        dto.WorkTaskId.Should().Be(workTaskId);
        dto.AgentInstanceId.Should().Be(agentInstanceId);
        dto.Type.Should().Be(AttemptType.Implementation);
        dto.ReviewNote.Should().BeNull();
        dto.TestNote.Should().BeNull();
        dto.CompletedAt.Should().BeNull();
    }
}
