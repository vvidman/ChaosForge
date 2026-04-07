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

public sealed class GetURSByIdQueryHandlerTests
{
    private readonly IURSRepository _ursRepository = Substitute.For<IURSRepository>();

    private GetURSByIdQueryHandler CreateHandler() =>
        new(_ursRepository);

    [Fact]
    public async Task Handle_WhenURSNotFound_ReturnsFailure()
    {
        // Arrange
        var ursId = Guid.NewGuid();
        _ursRepository.GetByIdAsync(ursId, Arg.Any<CancellationToken>())
            .Returns((URSEntity?)null);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetURSByIdQuery(ursId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("URS not found.");
    }

    [Fact]
    public async Task Handle_WhenURSFound_MapsAllFieldsCorrectly()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var urs = new URSEntity(useCaseId, "Login Requirement", "User must be able to log in");

        _ursRepository.GetByIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(urs);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetURSByIdQuery(urs.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var dto = result.Value!;
        dto.Id.Should().Be(urs.Id);
        dto.UseCaseId.Should().Be(useCaseId);
        dto.Title.Should().Be("Login Requirement");
        dto.Description.Should().Be("User must be able to log in");
        dto.HumanEditNote.Should().BeNull();
        dto.CreatedAt.Should().Be(urs.CreatedAt);
    }

    [Fact]
    public async Task Handle_WhenURSHasHumanEditNote_MapsHumanEditNoteCorrectly()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var urs = new URSEntity(useCaseId, "Login Requirement", "Original description");
        urs.ApplyHumanEdit("Revised description", "Clarified scope");

        _ursRepository.GetByIdAsync(urs.Id, Arg.Any<CancellationToken>())
            .Returns(urs);

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetURSByIdQuery(urs.Id), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value!.HumanEditNote.Should().Be("Clarified scope");
    }
}
