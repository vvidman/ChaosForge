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
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.WorkTasks.Queries;

public sealed class GetWorkTasksBySprintIdQueryHandlerTests
{
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();

    private GetWorkTasksBySprintIdQueryHandler CreateHandler() =>
        new(_workTaskRepository);

    [Fact]
    public async Task Handle_WhenNoTasksExist_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var sprintId = Guid.NewGuid();
        _workTaskRepository.GetBySprintIdAsync(sprintId, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask>().AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetWorkTasksBySprintIdQuery(sprintId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenTasksExist_MapsAllFieldsCorrectly()
    {
        // Arrange
        var srsId = Guid.NewGuid();
        var sprintId = Guid.NewGuid();
        var task = new WorkTask(srsId, "Write tests", "Unit test coverage", 2);
        task.AssignToSprint(sprintId);

        _workTaskRepository.GetBySprintIdAsync(sprintId, Arg.Any<CancellationToken>())
            .Returns(new List<WorkTask> { task }.AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetWorkTasksBySprintIdQuery(sprintId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        var dto = result.Value![0];
        dto.Id.Should().Be(task.Id);
        dto.SprintId.Should().Be(sprintId);
        dto.Title.Should().Be("Write tests");
        dto.StoryPoints.Should().Be(2);
        dto.CreatedAt.Should().Be(task.CreatedAt);
    }
}
