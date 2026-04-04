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

using ChaosForge.Domain.Entities;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Exceptions;
using FluentAssertions;

namespace ChaosForge.Domain.Tests.Entities;

public sealed class AgentSlotTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public void UpdateCount_ValidCountForNonSingletonRole_SetsCount(int count)
    {
        // Arrange
        var slot = new AgentSlot(Guid.NewGuid(), AgentRole.Developer, 1);

        // Act
        slot.UpdateCount(count);

        // Assert
        slot.Count.Should().Be(count);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-99)]
    public void UpdateCount_CountLessThanOne_ThrowsDomainException(int count)
    {
        // Arrange
        var slot = new AgentSlot(Guid.NewGuid(), AgentRole.Developer, 1);

        // Act
        var act = () => slot.UpdateCount(count);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(AgentRole.BusinessAnalyst)]
    [InlineData(AgentRole.Architect)]
    [InlineData(AgentRole.ScrumMaster)]
    public void UpdateCount_CountGreaterThanOneForSingletonRole_ThrowsDomainException(AgentRole singletonRole)
    {
        // Arrange
        var slot = new AgentSlot(Guid.NewGuid(), singletonRole, 1);

        // Act
        var act = () => slot.UpdateCount(2);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void SingletonRoles_ContainsExactlyExpectedRoles()
    {
        // Arrange
        AgentRole[] expected = [AgentRole.BusinessAnalyst, AgentRole.Architect, AgentRole.ScrumMaster];

        // Act / Assert
        AgentSlot.SingletonRoles.Should().BeEquivalentTo(expected);
    }
}
