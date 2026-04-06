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

using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.WorkTasks.Commands;

public sealed class PassWorkTaskTestingCommandHandlerTests
{
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private PassWorkTaskTestingCommandHandler CreateHandler() =>
        new(_workTaskRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenTaskIsInTesting_PassesTestingAndSavesChanges()
    {
        // Arrange
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 2);
        task.AssignToSprint(Guid.NewGuid());
        task.Start();
        task.SendToReview();
        task.Approve();
        var command = new PassWorkTaskTestingCommand(task.Id);
        _workTaskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        task.Status.Should().Be(WorkTaskStatus.InDocumentation);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskNotFound_ReturnsFailure()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        var command = new PassWorkTaskTestingCommand(taskId);
        _workTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>()).Returns((WorkTask?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Work task not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenTaskNotInTesting_ReturnsDomainExceptionMessage()
    {
        // Arrange
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 2);
        var command = new PassWorkTaskTestingCommand(task.Id);
        _workTaskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>()).Returns(task);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
