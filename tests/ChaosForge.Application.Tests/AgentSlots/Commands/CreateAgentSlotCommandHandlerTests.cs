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

using ChaosForge.Application.AgentSlots.Commands;
using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Repositories;
using FluentAssertions;
using NSubstitute;

namespace ChaosForge.Application.Tests.AgentSlots.Commands;

public sealed class CreateAgentSlotCommandHandlerTests
{
    private readonly IAgentSlotRepository _agentSlotRepository = Substitute.For<IAgentSlotRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private CreateAgentSlotCommandHandler CreateHandler() =>
        new(_agentSlotRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WithValidCommand_AddsAgentSlotAndSavesChanges()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var command = new CreateAgentSlotCommand(projectId, AgentRole.Developer, 3);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await _agentSlotRepository.Received(1).AddAsync(
            Arg.Is<AgentSlot>(s => s.ProjectId == projectId && s.Role == AgentRole.Developer && s.Count == 3),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
