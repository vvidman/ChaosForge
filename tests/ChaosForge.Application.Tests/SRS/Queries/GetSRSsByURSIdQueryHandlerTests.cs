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

using ChaosForge.Application.SRS.Queries;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;
using SRSEntity = ChaosForge.Domain.Entities.SRS;

namespace ChaosForge.Application.Tests.SRS.Queries;

public sealed class GetSRSsByURSIdQueryHandlerTests
{
    private readonly ISRSRepository _srsRepository = Substitute.For<ISRSRepository>();

    private GetSRSsByURSIdQueryHandler CreateHandler() =>
        new(_srsRepository);

    [Fact]
    public async Task Handle_WhenNoSRSsExist_ReturnsSuccessWithEmptyList()
    {
        // Arrange
        var ursId = Guid.NewGuid();
        _srsRepository.GetByURSIdAsync(ursId, Arg.Any<CancellationToken>())
            .Returns(new List<SRSEntity>().AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetSRSsByURSIdQuery(ursId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenSRSsExist_MapsAllFieldsCorrectly()
    {
        // Arrange
        var ursId = Guid.NewGuid();
        var srs = new SRSEntity(ursId, "Auth Service Spec", "Implement JWT-based authentication");

        _srsRepository.GetByURSIdAsync(ursId, Arg.Any<CancellationToken>())
            .Returns(new List<SRSEntity> { srs }.AsReadOnly());

        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(new GetSRSsByURSIdQuery(ursId), CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().ContainSingle();

        var dto = result.Value![0];
        dto.Id.Should().Be(srs.Id);
        dto.URSId.Should().Be(ursId);
        dto.Title.Should().Be("Auth Service Spec");
        dto.TechnicalDescription.Should().Be("Implement JWT-based authentication");
        dto.HumanEditNote.Should().BeNull();
        dto.CreatedAt.Should().Be(srs.CreatedAt);
    }
}
