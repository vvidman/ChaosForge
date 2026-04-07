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

public sealed class GetUseCaseByIdQueryHandlerTests
{
    private readonly IUseCaseRepository _useCaseRepository = Substitute.For<IUseCaseRepository>();

    private GetUseCaseByIdQueryHandler CreateHandler() =>
        new(_useCaseRepository);

    [Fact]
    public async Task Handle_WhenUseCaseNotFound_ReturnsFailure()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        _useCaseRepository.GetByIdAsync(useCaseId, Arg.Any<CancellationToken>())
            .Returns((UseCase?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetUseCaseByIdQuery(useCaseId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Use case not found.");
    }

    [Fact]
    public async Task Handle_WhenUseCaseFound_MapsAllFieldsCorrectly()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var useCase = new UseCase(projectId, "User Login", "Allow users to log in", 1);

        _useCaseRepository.GetByIdAsync(useCase.Id, Arg.Any<CancellationToken>())
            .Returns(useCase);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetUseCaseByIdQuery(useCase.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(useCase.Id);
        dto.ProjectId.Should().Be(projectId);
        dto.Title.Should().Be("User Login");
        dto.Description.Should().Be("Allow users to log in");
        dto.Priority.Should().Be(1);
        dto.CreatedAt.Should().Be(useCase.CreatedAt);
    }
}
