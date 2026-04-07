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

public sealed class ProjectTests
{
    [Fact]
    public void Constructor_Always_SetsStatusToSetup()
    {
        // Arrange / Act
        var project = new Project("My Project", "A description");

        // Assert
        project.Status.Should().Be(ProjectStatus.Setup);
    }

    [Fact]
    public void Constructor_Always_SetsCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var project = new Project("My Project", "A description");

        // Assert
        project.CreatedAt.Should().BeOnOrAfter(before);
    }

    [Theory]
    [InlineData(ProjectStatus.Setup, ProjectStatus.RequirementsPhase)]
    [InlineData(ProjectStatus.RequirementsPhase, ProjectStatus.ArchitecturePhase)]
    [InlineData(ProjectStatus.ArchitecturePhase, ProjectStatus.SprintPlanning)]
    [InlineData(ProjectStatus.SprintPlanning, ProjectStatus.Development)]
    [InlineData(ProjectStatus.Development, ProjectStatus.Completed)]
    public void TransitionTo_NextStatus_AdvancesCorrectly(ProjectStatus from, ProjectStatus to)
    {
        // Arrange
        var project = BuildProjectAtStatus(from);

        // Act
        project.TransitionTo(to);

        // Assert
        project.Status.Should().Be(to);
        var evt = project.DomainEvents.OfType<ProjectStatusChangedEvent>().Last();
        evt.ProjectId.Should().Be(project.Id);
        evt.OldStatus.Should().Be(from);
        evt.NewStatus.Should().Be(to);
    }

    [Theory]
    [InlineData(ProjectStatus.Setup, ProjectStatus.Setup)]
    [InlineData(ProjectStatus.Setup, ProjectStatus.ArchitecturePhase)]
    [InlineData(ProjectStatus.Setup, ProjectStatus.Completed)]
    [InlineData(ProjectStatus.RequirementsPhase, ProjectStatus.Setup)]
    [InlineData(ProjectStatus.Development, ProjectStatus.SprintPlanning)]
    public void TransitionTo_BackwardOrSkipStatus_ThrowsDomainException(ProjectStatus from, ProjectStatus to)
    {
        // Arrange
        var project = BuildProjectAtStatus(from);

        // Act
        var act = () => project.TransitionTo(to);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void TransitionTo_WhenAlreadyCompleted_ThrowsDomainException()
    {
        // Arrange
        var project = BuildProjectAtStatus(ProjectStatus.Completed);

        // Act
        var act = () => project.TransitionTo(ProjectStatus.Completed);

        // Assert
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void UpdateDescription_ValidValue_UpdatesDescription()
    {
        // Arrange
        var project = new Project("My Project", "Old description");

        // Act
        project.UpdateDescription("New description");

        // Assert
        project.Description.Should().Be("New description");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UpdateDescription_NullEmptyOrWhitespace_ThrowsDomainException(string? value)
    {
        // Arrange
        var project = new Project("My Project", "A description");

        // Act
        var act = () => project.UpdateDescription(value!);

        // Assert
        act.Should().Throw<DomainException>();
    }

    // Helper: advances a project to the target status by replaying transitions.
    private static Project BuildProjectAtStatus(ProjectStatus target)
    {
        ProjectStatus[] sequence =
        [
            ProjectStatus.Setup,
            ProjectStatus.RequirementsPhase,
            ProjectStatus.ArchitecturePhase,
            ProjectStatus.SprintPlanning,
            ProjectStatus.Development,
            ProjectStatus.Completed,
        ];

        var project = new Project("Test Project", "Description");

        for (var i = 0; i < sequence.Length - 1; i++)
        {
            if (sequence[i] == target)
            {
                break;
            }

            project.TransitionTo(sequence[i + 1]);
        }

        return project;
    }
}
