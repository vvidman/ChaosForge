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

public sealed class OpenRevisionGateCommandHandlerTests
{
    private readonly IRevisionGateRepository _revisionGateRepository = Substitute.For<IRevisionGateRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private OpenRevisionGateCommandHandler CreateHandler() =>
        new(_revisionGateRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenNoOpenGateExists_CreatesGateAndSavesChanges()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new OpenRevisionGateCommand(projectId, RevisionGateType.Requirements, "Agent output here.");
        _revisionGateRepository.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns((RevisionGate?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _revisionGateRepository.Received(1).AddAsync(
            Arg.Is<RevisionGate>(g => g.ProjectId == projectId && g.Type == RevisionGateType.Requirements),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenGateAlreadyOpen_ReturnsFailure()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var existing = new RevisionGate(projectId, RevisionGateType.Requirements, "Existing output.");
        var command = new OpenRevisionGateCommand(projectId, RevisionGateType.Architecture, "New output.");
        _revisionGateRepository.GetOpenByProjectIdAsync(projectId, Arg.Any<CancellationToken>())
            .Returns(existing);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("A revision gate is already open for this project.");
        await _revisionGateRepository.DidNotReceive().AddAsync(Arg.Any<RevisionGate>(), Arg.Any<CancellationToken>());
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
