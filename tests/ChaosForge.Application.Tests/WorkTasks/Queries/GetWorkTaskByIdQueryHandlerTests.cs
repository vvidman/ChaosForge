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

using ChaosForge.Application.WorkTasks.Queries;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.WorkTasks.Queries;

public sealed class GetWorkTaskByIdQueryHandlerTests
{
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();

    private GetWorkTaskByIdQueryHandler CreateHandler() =>
        new(_workTaskRepository);

    [Fact]
    public async Task Handle_WhenWorkTaskNotFound_ReturnsFailure()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        _workTaskRepository.GetByIdAsync(taskId, Arg.Any<CancellationToken>())
            .Returns((WorkTask?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetWorkTaskByIdQuery(taskId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Work task not found.");
    }

    [Fact]
    public async Task Handle_WhenWorkTaskFound_MapsAllFieldsCorrectly()
    {
        // Arrange
        var srsId = Guid.NewGuid();
        var task = new WorkTask(srsId, "Implement login", "Add login endpoint", 3);

        _workTaskRepository.GetByIdAsync(task.Id, Arg.Any<CancellationToken>())
            .Returns(task);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetWorkTaskByIdQuery(task.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(task.Id);
        dto.SRSId.Should().Be(srsId);
        dto.SprintId.Should().BeNull();
        dto.Title.Should().Be("Implement login");
        dto.Description.Should().Be("Add login endpoint");
        dto.Status.Should().Be(WorkTaskStatus.Backlog);
        dto.StoryPoints.Should().Be(3);
        dto.CreatedAt.Should().Be(task.CreatedAt);
    }
}
