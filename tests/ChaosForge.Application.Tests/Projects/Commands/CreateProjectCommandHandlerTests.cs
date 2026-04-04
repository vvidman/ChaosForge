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

using ChaosForge.Application.Projects.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.Projects.Commands;

public sealed class CreateProjectCommandHandlerTests
{
    private readonly IProjectRepository _projectRepository = Substitute.For<IProjectRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateProjectCommandHandler CreateHandler() =>
        new(_projectRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_AddsProjectAndSavesChanges()
    {
        // Arrange
        var command = new CreateProjectCommand("My Project", "A description", null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _projectRepository.Received(1).AddAsync(
            Arg.Is<Project>(p => p.Name == "My Project" && p.Description == "A description"),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithDeadline_PassesDeadlineToProject()
    {
        // Arrange
        var deadline = new DateTime(2026, 12, 31);
        var command = new CreateProjectCommand("My Project", "A description", deadline);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _projectRepository.Received(1).AddAsync(
            Arg.Is<Project>(p => p.Deadline == deadline),
            Arg.Any<CancellationToken>());
    }
}
