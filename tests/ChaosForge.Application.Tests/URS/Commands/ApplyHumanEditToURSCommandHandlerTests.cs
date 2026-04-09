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

using ChaosForge.Application.URS.Commands;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.URS.Commands;

public sealed class ApplyHumanEditToURSCommandHandlerTests
{
    private readonly IURSRepository _ursRepository = Substitute.For<IURSRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private ApplyHumanEditToURSCommandHandler CreateHandler() =>
        new(_ursRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenURSExists_AppliesEditAndSavesChanges()
    {
        // Arrange
        var urs = new Domain.Entities.URS(Guid.NewGuid(), "Login requirement", "Original description");
        var command = new ApplyHumanEditToURSCommand(urs.Id, "Revised description", "Clarified wording");
        _ursRepository.GetByIdAsync(urs.Id, Arg.Any<CancellationToken>()).Returns(urs);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        urs.Description.Should().Be("Revised description");
        urs.HumanEditNote.Should().Be("Clarified wording");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenURSNotFound_ReturnsFailure()
    {
        // Arrange
        var ursId = Guid.NewGuid();
        var command = new ApplyHumanEditToURSCommand(ursId, "Revised description", "Note");
        _ursRepository.GetByIdAsync(ursId, Arg.Any<CancellationToken>()).Returns((Domain.Entities.URS?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("URS not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenEditedDescriptionIsWhitespace_ReturnsDomainExceptionMessage()
    {
        // Arrange
        var urs = new Domain.Entities.URS(Guid.NewGuid(), "Login requirement", "Original description");
        var command = new ApplyHumanEditToURSCommand(urs.Id, "   ", "Note");
        _ursRepository.GetByIdAsync(urs.Id, Arg.Any<CancellationToken>()).Returns(urs);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
