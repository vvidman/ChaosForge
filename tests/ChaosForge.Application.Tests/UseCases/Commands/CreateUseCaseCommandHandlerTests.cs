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

public sealed class CreateUseCaseCommandHandlerTests
{
    private readonly IUseCaseRepository _useCaseRepository = Substitute.For<IUseCaseRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateUseCaseCommandHandler CreateHandler() =>
        new(_useCaseRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_AddsUseCaseAndSavesChanges()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new CreateUseCaseCommand(projectId, "Login", "User can log in", 1);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _useCaseRepository.Received(1).AddAsync(
            Arg.Is<UseCase>(u => u.ProjectId == projectId && u.Title == "Login" && u.Priority == 1),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
