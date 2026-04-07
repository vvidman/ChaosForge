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

using ChaosForge.Application.Projects.Queries;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.Projects.Queries;

public sealed class GetProjectByIdQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();

    private GetProjectByIdQueryHandler CreateHandler() =>
        new(_projectRepository);

    [Fact]
    public async Task Handle_WhenProjectNotFound_ReturnsFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _projectRepository.GetByIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((Project?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetProjectByIdQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Project not found.");
    }

    [Fact]
    public async Task Handle_WhenProjectFound_MapsAllFieldsCorrectly()
    {
        // Arrange
        var deadline = new DateTime(2026, 6, 30, 0, 0, 0, DateTimeKind.Utc);
        var project = new Project("Beta", "Second project", deadline);

        _projectRepository.GetByIdAsync(project.Id, Arg.Any<CancellationToken>())
            .Returns(project);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetProjectByIdQuery(project.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(project.Id);
        dto.Name.Should().Be("Beta");
        dto.Description.Should().Be("Second project");
        dto.Status.Should().Be(ProjectStatus.Setup);
        dto.Deadline.Should().Be(deadline);
        dto.CreatedAt.Should().Be(project.CreatedAt);
    }
}
