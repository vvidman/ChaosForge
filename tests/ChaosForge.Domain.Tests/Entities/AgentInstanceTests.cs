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

public sealed class AgentInstanceTests
{
    [Fact]
    public void Constructor_Always_SetsStatusToIdle()
    {
        // Arrange / Act
        var agent = new AgentInstance(Guid.NewGuid(), AgentRole.Developer, "Dev-1");

        // Assert
        agent.Status.Should().Be(AgentInstanceStatus.Idle);
    }

    [Fact]
    public void StartWork_WhenIdle_SetsCurrentTaskIdAndStatusToWorking()
    {
        // Arrange
        var agent = new AgentInstance(Guid.NewGuid(), AgentRole.Developer, "Dev-1");
        var taskId = Guid.NewGuid();

        // Act
        agent.StartWork(taskId);

        // Assert
        agent.CurrentTaskId.Should().Be(taskId);
        agent.Status.Should().Be(AgentInstanceStatus.Working);
    }

    [Fact]
    public void StartWork_WhenAlreadyWorking_ThrowsDomainException()
    {
        // Arrange
        var agent = new AgentInstance(Guid.NewGuid(), AgentRole.Developer, "Dev-1");
        agent.StartWork(Guid.NewGuid());

        // Act
        var act = () => agent.StartWork(Guid.NewGuid());

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FinishWork_Always_ClearsCurrentTaskIdAndSetsStatusToIdle()
    {
        // Arrange
        var agent = new AgentInstance(Guid.NewGuid(), AgentRole.Developer, "Dev-1");
        agent.StartWork(Guid.NewGuid());

        // Act
        agent.FinishWork();

        // Assert
        agent.CurrentTaskId.Should().BeNull();
        agent.Status.Should().Be(AgentInstanceStatus.Idle);
    }

    [Fact]
    public void Block_Always_SetsStatusToBlocked()
    {
        // Arrange
        var agent = new AgentInstance(Guid.NewGuid(), AgentRole.Developer, "Dev-1");

        // Act
        agent.Block();

        // Assert
        agent.Status.Should().Be(AgentInstanceStatus.Blocked);
    }

    [Fact]
    public void MarkFinished_Always_SetsStatusToFinished()
    {
        // Arrange
        var agent = new AgentInstance(Guid.NewGuid(), AgentRole.Developer, "Dev-1");

        // Act
        agent.MarkFinished();

        // Assert
        agent.Status.Should().Be(AgentInstanceStatus.Finished);
    }
}
