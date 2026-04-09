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

public sealed class CreateTaskAttemptCommandHandlerTests
{
    private readonly ITaskAttemptRepository _taskAttemptRepository = Substitute.For<ITaskAttemptRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateTaskAttemptCommandHandler CreateHandler() =>
        new(_taskAttemptRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_ReturnsSuccessWithNewAttemptId()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        var agentInstanceId = Guid.NewGuid();
        var command = new CreateTaskAttemptCommand(workTaskId, agentInstanceId, AttemptType.Implementation);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _taskAttemptRepository.Received(1).AddAsync(
            Arg.Is<TaskAttempt>(a =>
                a.WorkTaskId == workTaskId &&
                a.AgentInstanceId == agentInstanceId &&
                a.Type == AttemptType.Implementation &&
                a.Id == result.Value),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
