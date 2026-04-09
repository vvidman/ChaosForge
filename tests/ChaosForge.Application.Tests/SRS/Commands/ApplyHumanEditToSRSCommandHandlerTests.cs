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

using ChaosForge.Application.SRS.Commands;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.SRS.Commands;

public sealed class ApplyHumanEditToSRSCommandHandlerTests
{
    private readonly ISRSRepository _srsRepository = Substitute.For<ISRSRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ApplyHumanEditToSRSCommandHandler CreateHandler() =>
        new(_srsRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenSRSExists_AppliesEditAndSavesChanges()
    {
        // Arrange
        var srs = new Domain.Entities.SRS(Guid.NewGuid(), "Auth module spec", "Original technical description");
        var command = new ApplyHumanEditToSRSCommand(srs.Id, "Revised technical description", "Added JWT detail");
        _srsRepository.GetByIdAsync(srs.Id, Arg.Any<CancellationToken>()).Returns(srs);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        srs.TechnicalDescription.Should().Be("Revised technical description");
        srs.HumanEditNote.Should().Be("Added JWT detail");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenSRSNotFound_ReturnsFailure()
    {
        // Arrange
        var srsId = Guid.NewGuid();
        var command = new ApplyHumanEditToSRSCommand(srsId, "Revised description", "Note");
        _srsRepository.GetByIdAsync(srsId, Arg.Any<CancellationToken>()).Returns((Domain.Entities.SRS?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("SRS not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEditedDescriptionIsWhitespace_ReturnsDomainExceptionMessage()
    {
        // Arrange
        var srs = new Domain.Entities.SRS(Guid.NewGuid(), "Auth module spec", "Original technical description");
        var command = new ApplyHumanEditToSRSCommand(srs.Id, "   ", "Note");
        _srsRepository.GetByIdAsync(srs.Id, Arg.Any<CancellationToken>()).Returns(srs);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
