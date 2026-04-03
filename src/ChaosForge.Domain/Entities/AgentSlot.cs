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
/// Defines how many agents of a given role are provisioned for a project.
/// Singleton roles are enforced to a maximum of one instance.
/// </summary>
public sealed class AgentSlot : EntityBase<Guid>
{
    /// <summary>
    /// The set of roles that must always have exactly one agent instance (count cannot exceed 1).
    /// </summary>
    public static readonly IReadOnlySet<AgentRole> SingletonRoles = new HashSet<AgentRole>
    {
        AgentRole.BusinessAnalyst,
        AgentRole.Architect,
        AgentRole.ScrumMaster,
    };

    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private AgentSlot()
    {
    }

    /// <summary>
    /// Initializes a new agent slot for the given project and role.
    /// </summary>
    /// <param name="projectId">The identifier of the owning project.</param>
    /// <param name="role">The role this slot represents.</param>
    /// <param name="count">The initial number of agent instances. Must be >= 1.</param>
    public AgentSlot(Guid projectId, AgentRole role, int count) : base(Guid.NewGuid())
    {
        ProjectId = projectId;
        Role = role;
        Count = count;
    }

    /// <summary>Gets the identifier of the owning project.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the agent role this slot represents.</summary>
    public AgentRole Role { get; private set; }

    /// <summary>Gets the number of agent instances allocated for this slot.</summary>
    public int Count { get; private set; }

    /// <summary>
    /// Updates the number of agent instances for this slot.
    /// </summary>
    /// <param name="count">The new count. Must be >= 1. Singleton roles may not exceed 1.</param>
    /// <exception cref="DomainException">Thrown when count is less than 1, or role is a singleton and count exceeds 1.</exception>
    public void UpdateCount(int count)
    {
        if (count < 1)
        {
            throw new DomainException($"Agent slot count must be >= 1, but was {count}.");
        }

        if (SingletonRoles.Contains(Role) && count > 1)
        {
            throw new DomainException($"Role {Role} is a singleton and cannot have more than 1 agent instance.");
        }

        Count = count;
    }
}
