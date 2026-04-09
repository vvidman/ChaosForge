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

public sealed class UpdateAgentSlotCountCommandHandlerTests
{
    private readonly IAgentSlotRepository _agentSlotRepository = Substitute.For<IAgentSlotRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();

    private UpdateAgentSlotCountCommandHandler CreateHandler() =>
        new(_agentSlotRepository, _unitOfWork);

    [Fact]
    public async Task Handle_WhenAgentSlotExists_UpdatesCountAndSavesChanges()
    {
        // Arrange
        var agentSlot = new AgentSlot(Guid.NewGuid(), AgentRole.Developer, 2);
        var command = new UpdateAgentSlotCountCommand(agentSlot.Id, 5);
        _agentSlotRepository.GetByIdAsync(agentSlot.Id, Arg.Any<CancellationToken>()).Returns(agentSlot);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        agentSlot.Count.Should().Be(5);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenAgentSlotNotFound_ReturnsFailure()
    {
        // Arrange
        var agentSlotId = Guid.NewGuid();
        var command = new UpdateAgentSlotCountCommand(agentSlotId, 3);
        _agentSlotRepository.GetByIdAsync(agentSlotId, Arg.Any<CancellationToken>()).Returns((AgentSlot?)null);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be("AgentSlot not found.");
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WhenCountViolatesSingletonRule_ReturnsFailure()
    {
        // Arrange
        var agentSlot = new AgentSlot(Guid.NewGuid(), AgentRole.BusinessAnalyst, 1);
        var command = new UpdateAgentSlotCountCommand(agentSlot.Id, 2);
        _agentSlotRepository.GetByIdAsync(agentSlot.Id, Arg.Any<CancellationToken>()).Returns(agentSlot);
        var handler = CreateHandler();

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().NotBeNullOrEmpty();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
