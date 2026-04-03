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
using ChaosForge.Domain.Exceptions;

namespace ChaosForge.Domain.Entities;

/// <summary>
/// Represents a use case belonging to a project, capturing a user goal or system behaviour.
/// </summary>
public sealed class UseCase : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private UseCase()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Initializes a new use case for the given project.
    /// </summary>
    /// <param name="projectId">The identifier of the owning project.</param>
    /// <param name="title">The short title of the use case.</param>
    /// <param name="description">A detailed description of the use case.</param>
    /// <param name="priority">The relative priority; must be >= 0.</param>
    public UseCase(Guid projectId, string title, string description, int priority) : base(Guid.NewGuid())
    {
        ProjectId = projectId;
        Title = title;
        Description = description;
        Priority = priority;
    }

    /// <summary>Gets the identifier of the owning project.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the short title of the use case.</summary>
    public string Title { get; private set; }

    /// <summary>Gets the detailed description of the use case.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the relative priority of the use case (non-negative integer).</summary>
    public int Priority { get; private set; }

    /// <summary>
    /// Updates the priority of this use case.
    /// </summary>
    /// <param name="priority">The new priority value. Must be >= 0.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="priority"/> is negative.</exception>
    public void UpdatePriority(int priority)
    {
        if (priority < 0)
        {
            throw new DomainException($"Priority must be >= 0, but was {priority}.");
        }

        Priority = priority;
    }
}
