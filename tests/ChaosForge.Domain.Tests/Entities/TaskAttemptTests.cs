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

public sealed class TaskAttemptTests
{
    [Fact]
    public void Constructor_Always_SetsResultToPending()
    {
        // Arrange / Act
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);

        // Assert
        attempt.Result.Should().Be(AttemptResult.Pending);
    }

    [Fact]
    public void Constructor_Always_SetsStartedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);

        // Assert
        attempt.StartedAt.Should().BeOnOrAfter(before);
    }

    [Fact]
    public void Constructor_Always_SetsOutputToEmpty()
    {
        // Arrange / Act
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);

        // Assert
        attempt.Output.Should().BeEmpty();
    }

    [Fact]
    public void Complete_ValidOutput_SetsOutputAndCompletedAt()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        var attempt = new TaskAttempt(workTaskId, Guid.NewGuid(), AttemptType.Implementation);
        var before = DateTime.UtcNow;

        // Act
        attempt.Complete("The agent output");

        // Assert
        attempt.Output.Should().Be("The agent output");
        attempt.CompletedAt.Should().NotBeNull();
        attempt.CompletedAt!.Value.Should().BeOnOrAfter(before);
        var evt = attempt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskAttemptCompletedEvent>().Subject;
        evt.TaskAttemptId.Should().Be(attempt.Id);
        evt.WorkTaskId.Should().Be(workTaskId);
        evt.Type.Should().Be(AttemptType.Implementation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Complete_NullEmptyOrWhitespace_ThrowsDomainException(string? output)
    {
        // Arrange
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);

        // Act
        var act = () => attempt.Complete(output!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Complete_CalledTwice_ThrowsDomainException()
    {
        // Arrange
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Implementation);
        attempt.Complete("First output");

        // Act
        var act = () => attempt.Complete("Second output");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Approve_WhenPending_SetsResultToApproved()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        var attempt = new TaskAttempt(workTaskId, Guid.NewGuid(), AttemptType.Implementation);

        // Act
        attempt.Approve();

        // Assert
        attempt.Result.Should().Be(AttemptResult.Approved);
        var evt = attempt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskAttemptResolvedEvent>().Subject;
        evt.TaskAttemptId.Should().Be(attempt.Id);
        evt.WorkTaskId.Should().Be(workTaskId);
        evt.Result.Should().Be(AttemptResult.Approved);
    }

    [Theory]
    [InlineData(AttemptResult.Approved)]
    [InlineData(AttemptResult.Rejected)]
    public void Approve_WhenAlreadyResolved_ThrowsDomainException(AttemptResult resolvedResult)
    {
        // Arrange
        var attempt = AttemptResolvedWith(resolvedResult);

        // Act
        var act = () => attempt.Approve();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_WithReviewType_SetsReviewNote()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        var attempt = new TaskAttempt(workTaskId, Guid.NewGuid(), AttemptType.Review);

        // Act
        attempt.Reject("Not good enough");

        // Assert
        attempt.ReviewNote.Should().Be("Not good enough");
        attempt.Result.Should().Be(AttemptResult.Rejected);
        var evt = attempt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskAttemptResolvedEvent>().Subject;
        evt.TaskAttemptId.Should().Be(attempt.Id);
        evt.WorkTaskId.Should().Be(workTaskId);
        evt.Result.Should().Be(AttemptResult.Rejected);
    }

    [Fact]
    public void Reject_WithTestingType_SetsTestNote()
    {
        // Arrange
        var workTaskId = Guid.NewGuid();
        var attempt = new TaskAttempt(workTaskId, Guid.NewGuid(), AttemptType.Testing);

        // Act
        attempt.Reject("Tests failed");

        // Assert
        attempt.TestNote.Should().Be("Tests failed");
        attempt.Result.Should().Be(AttemptResult.Rejected);
        var evt = attempt.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<TaskAttemptResolvedEvent>().Subject;
        evt.TaskAttemptId.Should().Be(attempt.Id);
        evt.WorkTaskId.Should().Be(workTaskId);
        evt.Result.Should().Be(AttemptResult.Rejected);
    }

    [Theory]
    [InlineData(AttemptType.Implementation)]
    [InlineData(AttemptType.Documentation)]
    public void Reject_WithUnsupportedType_ThrowsDomainException(AttemptType unsupported)
    {
        // Arrange
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), unsupported);

        // Act
        var act = () => attempt.Reject("some note");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(AttemptResult.Approved)]
    [InlineData(AttemptResult.Rejected)]
    public void Reject_WhenAlreadyResolved_ThrowsDomainException(AttemptResult resolvedResult)
    {
        // Arrange
        var attempt = AttemptResolvedWith(resolvedResult);

        // Act
        var act = () => attempt.Reject("note");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_NullEmptyOrWhitespaceNote_ThrowsDomainException(string? note)
    {
        // Arrange
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Review);

        // Act
        var act = () => attempt.Reject(note!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // Helpers

    private static TaskAttempt AttemptResolvedWith(AttemptResult result)
    {
        var attempt = new TaskAttempt(Guid.NewGuid(), Guid.NewGuid(), AttemptType.Review);

        if (result == AttemptResult.Approved)
        {
            attempt.Approve();
        }
        else
        {
            attempt.Reject("rejection note");
        }

        return attempt;
    }
}
