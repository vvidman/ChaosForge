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

using ChaosForge.Domain.Common;
using ChaosForge.Domain.Enums;
using ChaosForge.Domain.Exceptions;

namespace ChaosForge.Domain.Entities;

/// <summary>
/// Represents a software project managed by the ChaosForge multi-agent team.
/// </summary>
public sealed class Project : EntityBase<Guid>
{
    private static readonly ProjectStatus[] StatusSequence =
    [
        ProjectStatus.Setup,
        ProjectStatus.RequirementsPhase,
        ProjectStatus.ArchitecturePhase,
        ProjectStatus.SprintPlanning,
        ProjectStatus.Development,
        ProjectStatus.Completed,
    ];

    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private Project()
    {
        Name = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Initializes a new project in the <see cref="ProjectStatus.Setup"/> phase.
    /// </summary>
    /// <param name="name">The display name of the project.</param>
    /// <param name="description">A brief description of the project.</param>
    /// <param name="deadline">Optional deadline for the project.</param>
    public Project(string name, string description, DateTime? deadline = null) : base(Guid.NewGuid())
    {
        Name = name;
        Description = description;
        Status = ProjectStatus.Setup;
        Deadline = deadline;
    }

    /// <summary>Gets the display name of the project.</summary>
    public string Name { get; private set; }

    /// <summary>Gets the description of the project.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the current lifecycle phase of the project.</summary>
    public ProjectStatus Status { get; private set; }

    /// <summary>Gets the optional deadline for the project.</summary>
    public DateTime? Deadline { get; private set; }

    /// <summary>
    /// Updates the project description.
    /// </summary>
    /// <param name="description">The new description.</param>
    public void UpdateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException($"{nameof(description)} must not be null or whitespace.");
        }

        Description = description;
    }

    /// <summary>
    /// Advances the project to the next status in the linear sequence.
    /// Transitions are forward-only: Setup → RequirementsPhase → ArchitecturePhase → SprintPlanning → Development → Completed.
    /// </summary>
    /// <param name="newStatus">The target status.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="newStatus"/> is not the immediate next step.</exception>
    public void TransitionTo(ProjectStatus newStatus)
    {
        var currentIndex = Array.IndexOf(StatusSequence, Status);
        var targetIndex = Array.IndexOf(StatusSequence, newStatus);

        if (targetIndex != currentIndex + 1)
        {
            throw new DomainException(
                $"Cannot transition project from {Status} to {newStatus}. Only forward sequential transitions are allowed.");
        }

        Status = newStatus;
    }
}
