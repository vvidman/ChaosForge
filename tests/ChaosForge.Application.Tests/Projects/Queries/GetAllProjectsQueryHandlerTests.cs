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

public sealed class GetAllProjectsQueryHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();

    private GetAllProjectsQueryHandler CreateHandler() =>
        new(_projectRepository);

    [Fact]
    public async Task Handle_WhenRepositoryReturnsEmpty_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        _projectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project>().AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetAllProjectsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenRepositoryReturnsProjects_MapsAllFieldsCorrectly()
    {
        // Arrange
        var deadline = new DateTime(2026, 12, 31, 0, 0, 0, DateTimeKind.Utc);
        var project = new Project("Alpha", "First project", deadline);

        _projectRepository.GetAllAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Project> { project }.AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetAllProjectsQuery(), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        var dto = result.Value![0];
        dto.Id.Should().Be(project.Id);
        dto.Name.Should().Be("Alpha");
        dto.Description.Should().Be("First project");
        dto.Status.Should().Be(ProjectStatus.Setup);
        dto.Deadline.Should().Be(deadline);
        dto.CreatedAt.Should().Be(project.CreatedAt);
    }
}
