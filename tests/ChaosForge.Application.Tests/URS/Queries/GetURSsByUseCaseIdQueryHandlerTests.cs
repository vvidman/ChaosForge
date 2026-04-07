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

using ChaosForge.Application.URS.Queries;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using URSEntity = ChaosForge.Domain.Entities.URS;

namespace ChaosForge.Application.Tests.URS.Queries;

public sealed class GetURSsByUseCaseIdQueryHandlerTests
{
    private readonly IURSRepository _ursRepository = Substitute.For<IURSRepository>();

    private GetURSsByUseCaseIdQueryHandler CreateHandler() =>
        new(_ursRepository);

    [Fact]
    public async Task Handle_WhenNoURSsExist_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        _ursRepository.GetByUseCaseIdAsync(useCaseId, Arg.Any<CancellationToken>())
            .Returns(new List<URSEntity>().AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetURSsByUseCaseIdQuery(useCaseId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenURSsExist_MapsAllFieldsCorrectly()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var urs = new URSEntity(useCaseId, "Login Requirement", "User must be able to log in");

        _ursRepository.GetByUseCaseIdAsync(useCaseId, Arg.Any<CancellationToken>())
            .Returns(new List<URSEntity> { urs }.AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetURSsByUseCaseIdQuery(useCaseId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        var dto = result.Value![0];
        dto.Id.Should().Be(urs.Id);
        dto.UseCaseId.Should().Be(useCaseId);
        dto.Title.Should().Be("Login Requirement");
        dto.HumanEditNote.Should().BeNull();
        dto.CreatedAt.Should().Be(urs.CreatedAt);
    }
}
