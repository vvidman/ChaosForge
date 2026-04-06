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

using ChaosForge.Application.Common;
using ChaosForge.Application.RevisionGates.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.RevisionGates.Commands;

public sealed class EditAndAcceptRevisionGateCommandHandlerTests
{
    private readonly IRevisionGateRepository _revisionGateRepository = Substitute.For<IRevisionGateRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private EditAndAcceptRevisionGateCommandHandler CreateHandler() =>
        new(_revisionGateRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenGateIsOpen_EditAndAcceptsAndSavesChanges()
    {
        // Arrange
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "Agent output.");
        var command = new EditAndAcceptRevisionGateCommand(gate.Id, "Human edited output.");
        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        gate.Status.Should().Be(RevisionGateStatus.Resolved);
        gate.Action.Should().Be(RevisionGateAction.EditAndAccept);
        gate.HumanEditedOutput.Should().Be("Human edited output.");
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGateNotFound_ReturnsFailure()
    {
        // Arrange
        var gateId = Guid.NewGuid();
        var command = new EditAndAcceptRevisionGateCommand(gateId, "Human edited output.");
        _revisionGateRepository.GetByIdAsync(gateId, Arg.Any<CancellationToken>()).Returns((RevisionGate?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("Revision gate not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGateAlreadyResolved_ReturnsDomainExceptionMessage()
    {
        // Arrange
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "Agent output.");
        gate.Accept();
        var command = new EditAndAcceptRevisionGateCommand(gate.Id, "Human edited output.");
        _revisionGateRepository.GetByIdAsync(gate.Id, Arg.Any<CancellationToken>()).Returns(gate);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
