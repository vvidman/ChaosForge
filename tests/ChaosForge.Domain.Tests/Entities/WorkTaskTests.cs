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
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Exceptions;
using FluentAssertions;

namespace ChaosForge.Domain.Tests.Entities;

public sealed class WorkTaskTests
{
    private static readonly Guid SomeSprintId = Guid.NewGuid();

    [Fact]
    public void Constructor_Always_SetsStatusToBacklog()
    {
        // Arrange / Act
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);

        // Assert
        task.Status.Should().Be(WorkTaskStatus.Backlog);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespaceTitle_ThrowsDomainException(string? title)
    {
        // Arrange / Act
        var act = () => new WorkTask(Guid.NewGuid(), title!, "Description", 3);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrWhitespaceDescription_ThrowsDomainException(string? description)
    {
        // Arrange / Act
        var act = () => new WorkTask(Guid.NewGuid(), "Title", description!, 3);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AssignToSprint_WhenInBacklog_SetsSprintId()
    {
        // Arrange
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);

        // Act
        task.AssignToSprint(SomeSprintId);

        // Assert
        task.SprintId.Should().Be(SomeSprintId);
    }

    [Fact]
    public void AssignToSprint_WhenNotInBacklog_ThrowsDomainException()
    {
        // Arrange
        var task = TaskInProgress();

        // Act
        var act = () => task.AssignToSprint(SomeSprintId);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Start_WhenBacklogWithSprintAssigned_TransitionsToInProgress()
    {
        // Arrange
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);
        task.AssignToSprint(SomeSprintId);

        // Act
        task.Start();

        // Assert
        task.Status.Should().Be(WorkTaskStatus.InProgress);
        var evt = task.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<WorkTaskStatusChangedEvent>().Subject;
        evt.WorkTaskId.Should().Be(task.Id);
        evt.OldStatus.Should().Be(WorkTaskStatus.Backlog);
        evt.NewStatus.Should().Be(WorkTaskStatus.InProgress);
    }

    [Fact]
    public void Start_WhenNoSprintAssigned_ThrowsDomainException()
    {
        // Arrange
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);

        // Act
        var act = () => task.Start();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Start_WhenNotInBacklog_ThrowsDomainException()
    {
        // Arrange
        var task = TaskInProgress();

        // Act
        var act = () => task.Start();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void FullHappyPath_BacklogToDone_TransitionsCorrectly()
    {
        // Arrange
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);
        task.AssignToSprint(SomeSprintId);

        // Act
        task.Start();
        task.SendToReview();
        task.SendToTesting();
        task.PassTesting();
        task.Complete();

        // Assert
        task.Status.Should().Be(WorkTaskStatus.Done);
    }

    [Fact]
    public void SendToTesting_IsEquivalentToApprove()
    {
        // Arrange
        var taskA = TaskInReview();
        var taskB = TaskInReview();

        // Act
        taskA.SendToTesting();
        taskB.Approve();

        // Assert
        taskA.Status.Should().Be(taskB.Status);
    }

    [Fact]
    public void Reject_WhenInReview_ResetsSprintIdAndReturnsToBacklog()
    {
        // Arrange
        var task = TaskInReview();

        // Act
        task.Reject();

        // Assert
        task.Status.Should().Be(WorkTaskStatus.Backlog);
        task.SprintId.Should().BeNull();
        var evt = task.DomainEvents.OfType<WorkTaskStatusChangedEvent>().Last();
        evt.OldStatus.Should().Be(WorkTaskStatus.InReview);
        evt.NewStatus.Should().Be(WorkTaskStatus.Backlog);
    }

    [Fact]
    public void Reject_WhenInTesting_ResetsSprintIdAndReturnsToBacklog()
    {
        // Arrange
        var task = TaskInTesting();

        // Act
        task.Reject();

        // Assert
        task.Status.Should().Be(WorkTaskStatus.Backlog);
        task.SprintId.Should().BeNull();
        var evt = task.DomainEvents.OfType<WorkTaskStatusChangedEvent>().Last();
        evt.OldStatus.Should().Be(WorkTaskStatus.InTesting);
        evt.NewStatus.Should().Be(WorkTaskStatus.Backlog);
    }

    [Theory]
    [InlineData(WorkTaskStatus.InProgress)]
    [InlineData(WorkTaskStatus.InDocumentation)]
    [InlineData(WorkTaskStatus.Done)]
    public void Reject_WhenInNonRejectableStatus_ThrowsDomainException(WorkTaskStatus status)
    {
        // Arrange
        var task = BuildTaskAtStatus(status);

        // Act
        var act = () => task.Reject();

        // Assert
        act.Should().Throw<DomainException>();
    }

    // Helpers

    private static WorkTask TaskInProgress()
    {
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);
        task.AssignToSprint(SomeSprintId);
        task.Start();

        return task;
    }

    private static WorkTask TaskInReview()
    {
        var task = TaskInProgress();
        task.SendToReview();

        return task;
    }

    private static WorkTask TaskInTesting()
    {
        var task = TaskInReview();
        task.Approve();

        return task;
    }

    private static WorkTask BuildTaskAtStatus(WorkTaskStatus target)
    {
        var task = new WorkTask(Guid.NewGuid(), "Title", "Description", 3);
        task.AssignToSprint(SomeSprintId);

        if (target == WorkTaskStatus.Backlog)
        {
            return task;
        }

        task.Start();
        if (target == WorkTaskStatus.InProgress)
        {
            return task;
        }

        task.SendToReview();
        if (target == WorkTaskStatus.InReview)
        {
            return task;
        }

        task.Approve();
        if (target == WorkTaskStatus.InTesting)
        {
            return task;
        }

        task.PassTesting();
        if (target == WorkTaskStatus.InDocumentation)
        {
            return task;
        }

        task.Complete();

        return task;
    }
}
