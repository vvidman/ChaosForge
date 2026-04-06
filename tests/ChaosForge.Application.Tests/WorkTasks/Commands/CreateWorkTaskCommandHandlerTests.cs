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

using ChaosForge.Application.WorkTasks.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.WorkTasks.Commands;

public sealed class CreateWorkTaskCommandHandlerTests
{
    private readonly IWorkTaskRepository _workTaskRepository = Substitute.For<IWorkTaskRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateWorkTaskCommandHandler CreateHandler() =>
        new(_workTaskRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_AddsWorkTaskAndSavesChanges()
    {
        // Arrange
        var srsId = Guid.NewGuid();
        var command = new CreateWorkTaskCommand(srsId, "Implement login", "Add JWT login endpoint", 3);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _workTaskRepository.Received(1).AddAsync(
            Arg.Is<WorkTask>(t => t.SRSId == srsId && t.Title == "Implement login" && t.StoryPoints == 3),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
