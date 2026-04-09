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

using ChaosForge.Application.UseCases.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.UseCases.Commands;

public sealed class UpdateUseCasePriorityCommandHandlerTests
{
    private readonly IUseCaseRepository _useCaseRepository = Substitute.For<IUseCaseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private UpdateUseCasePriorityCommandHandler CreateHandler() =>
        new(_useCaseRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenUseCaseExists_UpdatesPriorityAndSavesChanges()
    {
        // Arrange
        var useCase = new UseCase(Guid.NewGuid(), "Login", "Description", 0);
        var command = new UpdateUseCasePriorityCommand(useCase.Id, 5);
        _useCaseRepository.GetByIdAsync(useCase.Id, Arg.Any<CancellationToken>()).Returns(useCase);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        useCase.Priority.Should().Be(5);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenUseCaseNotFound_ReturnsFailure()
    {
        // Arrange
        var useCaseId = Guid.NewGuid();
        var command = new UpdateUseCasePriorityCommand(useCaseId, 5);
        _useCaseRepository.GetByIdAsync(useCaseId, Arg.Any<CancellationToken>()).Returns((UseCase?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("UseCase not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenPriorityIsNegative_ReturnsDomainExceptionMessage()
    {
        // Arrange
        var useCase = new UseCase(Guid.NewGuid(), "Login", "Description", 0);
        var command = new UpdateUseCasePriorityCommand(useCase.Id, -1);
        _useCaseRepository.GetByIdAsync(useCase.Id, Arg.Any<CancellationToken>()).Returns(useCase);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
