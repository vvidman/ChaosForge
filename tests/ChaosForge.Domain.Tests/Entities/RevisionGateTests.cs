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

public sealed class RevisionGateTests
{
    [Fact]
    public void Constructor_Always_SetsStatusToOpen()
    {
        // Arrange / Act
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "agent output");

        // Assert
        gate.Status.Should().Be(RevisionGateStatus.Open);
    }

    [Fact]
    public void Accept_WhenOpen_SetsActionAndResolvesGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Requirements, "agent output");
        var before = DateTime.UtcNow;

        // Act
        gate.Accept();

        // Assert
        gate.Action.Should().Be(RevisionGateAction.Accept);
        gate.Status.Should().Be(RevisionGateStatus.Resolved);
        gate.ResolvedAt.Should().NotBeNull();
        gate.ResolvedAt!.Value.Should().BeOnOrAfter(before);
        var evt = gate.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RevisionGateResolvedEvent>().Subject;
        evt.RevisionGateId.Should().Be(gate.Id);
        evt.ProjectId.Should().Be(projectId);
        evt.Action.Should().Be(RevisionGateAction.Accept);
    }

    [Fact]
    public void EditAndAccept_ValidOutput_SetsHumanEditedOutputAndResolvesGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Requirements, "agent output");

        // Act
        gate.EditAndAccept("human-edited output");

        // Assert
        gate.HumanEditedOutput.Should().Be("human-edited output");
        gate.Action.Should().Be(RevisionGateAction.EditAndAccept);
        gate.Status.Should().Be(RevisionGateStatus.Resolved);
        var evt = gate.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RevisionGateResolvedEvent>().Subject;
        evt.RevisionGateId.Should().Be(gate.Id);
        evt.ProjectId.Should().Be(projectId);
        evt.Action.Should().Be(RevisionGateAction.EditAndAccept);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EditAndAccept_NullEmptyOrWhitespace_ThrowsDomainException(string? editedOutput)
    {
        // Arrange
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "agent output");

        // Act
        var act = () => gate.EditAndAccept(editedOutput!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_ValidReason_SetsRejectionReasonAndResolvesGate()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        var gate = new RevisionGate(projectId, RevisionGateType.Requirements, "agent output");

        // Act
        gate.Reject("Needs more detail");

        // Assert
        gate.RejectionReason.Should().Be("Needs more detail");
        gate.Action.Should().Be(RevisionGateAction.Reject);
        gate.Status.Should().Be(RevisionGateStatus.Resolved);
        var evt = gate.DomainEvents.Should().ContainSingle().Which.Should().BeOfType<RevisionGateResolvedEvent>().Subject;
        evt.RevisionGateId.Should().Be(gate.Id);
        evt.ProjectId.Should().Be(projectId);
        evt.Action.Should().Be(RevisionGateAction.Reject);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Reject_NullEmptyOrWhitespace_ThrowsDomainException(string? reason)
    {
        // Arrange
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "agent output");

        // Act
        var act = () => gate.Reject(reason!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Accept_WhenAlreadyResolved_ThrowsDomainException()
    {
        // Arrange
        var gate = ResolvedGate();

        // Act
        var act = () => gate.Accept();

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void EditAndAccept_WhenAlreadyResolved_ThrowsDomainException()
    {
        // Arrange
        var gate = ResolvedGate();

        // Act
        var act = () => gate.EditAndAccept("some edit");

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Reject_WhenAlreadyResolved_ThrowsDomainException()
    {
        // Arrange
        var gate = ResolvedGate();

        // Act
        var act = () => gate.Reject("reason");

        // Assert
        act.Should().Throw<DomainException>();
    }

    // Helper

    private static RevisionGate ResolvedGate()
    {
        var gate = new RevisionGate(Guid.NewGuid(), RevisionGateType.Requirements, "agent output");
        gate.Accept();

        return gate;
    }
}
