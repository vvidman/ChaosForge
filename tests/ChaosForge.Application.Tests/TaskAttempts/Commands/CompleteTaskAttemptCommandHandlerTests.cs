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

using ChaosForge.Application.TaskAttempts.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.TaskAttempts.Commands;

public sealed class CompleteTaskAttemptCommandHandlerTests
{
    private readonly ITaskAttemptRepository _taskAttemptRepository = Substitute.For<ITaskAttemptRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CompleteTaskAttemptCommandHandler CreateHandler() =>
        new(_taskAttemptRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenAttemptExists_CompletesAttemptAndSavesChanges()
    {
        // Arrange
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);
        var command = new CompleteTaskAttemptCommand(attempt.Id, "Agent output here.");
        _taskAttemptRepository.GetByIdAsync(attempt.Id, Arg.Any<CancellationToken>()).Returns(attempt);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        attempt.Output.Should().Be("Agent output here.");
        attempt.CompletedAt.Should().NotBeNull();
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAttemptNotFound_ReturnsFailure()
    {
        // Arrange
        var attemptId = Guid.NewGuid();
        var command = new CompleteTaskAttemptCommand(attemptId, "output");
        _taskAttemptRepository.GetByIdAsync(attemptId, Arg.Any<CancellationToken>()).Returns((TaskAttempt?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("TaskAttempt not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAttemptAlreadyCompleted_ReturnsDomainExceptionMessage()
    {
        // Arrange
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);
        attempt.Complete("first output");
        var command = new CompleteTaskAttemptCommand(attempt.Id, "second output");
        _taskAttemptRepository.GetByIdAsync(attempt.Id, Arg.Any<CancellationToken>()).Returns(attempt);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
