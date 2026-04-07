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

using ChaosForge.Application.UseCases.Queries;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.UseCases.Queries;

public sealed class GetUseCasesByProjectIdQueryHandlerTests
{
    private readonly IUseCaseRepository _useCaseRepository = Substitute.For<IUseCaseRepository>();

    private GetUseCasesByProjectIdQueryHandler CreateHandler() =>
        new(_useCaseRepository);

    [Fact]
    public async Task Handle_WhenNoUseCasesExist_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _useCaseRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase>().AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetUseCasesByProjectIdQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenUseCasesExist_MapsAllFieldsCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var useCase = new UseCase(projectId, "User Login", "Allow users to log in", 2);

        _useCaseRepository.GetByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(new List<UseCase> { useCase }.AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetUseCasesByProjectIdQuery(projectId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        var dto = result.Value![0];
        dto.Id.Should().Be(useCase.Id);
        dto.ProjectId.Should().Be(projectId);
        dto.Title.Should().Be("User Login");
        dto.Priority.Should().Be(2);
        dto.CreatedAt.Should().Be(useCase.CreatedAt);
    }
}
