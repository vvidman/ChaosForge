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
/// Represents a running instance of an AI agent with a specific role and persona within a project.
/// </summary>
public sealed class AgentInstance : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private AgentInstance()
    {
        PersonaName = string.Empty;
    }

    /// <summary>
    /// Initializes a new agent instance in the <see cref="AgentInstanceStatus.Idle"/> state.
    /// </summary>
    /// <param name="projectId">The identifier of the project this agent belongs to.</param>
    /// <param name="role">The functional role of this agent.</param>
    /// <param name="personaName">The persona name used when communicating with the LLM.</param>
    public AgentInstance(Guid projectId, AgentRole role, string personaName) : base(Guid.NewGuid())
    {
        ProjectId = projectId;
        Role = role;
        PersonaName = personaName;
        Status = AgentInstanceStatus.Idle;
    }

    /// <summary>Gets the identifier of the project this agent belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the functional role of this agent.</summary>
    public AgentRole Role { get; private set; }

    /// <summary>Gets the persona name used when communicating with the LLM.</summary>
    public string PersonaName { get; private set; }

    /// <summary>Gets the current operational status of this agent. Initialized to <see cref="AgentInstanceStatus.Idle"/>.</summary>
    public AgentInstanceStatus Status { get; private set; }

    /// <summary>Gets the identifier of the task this agent is currently working on, if any.</summary>
    public Guid? CurrentTaskId { get; private set; }

    /// <summary>
    /// Assigns a task to this agent and sets its status to <see cref="AgentInstanceStatus.Working"/>.
    /// </summary>
    /// <param name="taskId">The identifier of the task to work on.</param>
    /// <exception cref="DomainException">Thrown when the agent is already working.</exception>
    public void StartWork(Guid taskId)
    {
        if (Status is AgentInstanceStatus.Working)
        {
            throw new DomainException($"Agent {PersonaName} is already working on task {CurrentTaskId}.");
        }

        CurrentTaskId = taskId;
        Status = AgentInstanceStatus.Working;
    }

    /// <summary>
    /// Clears the current task and returns the agent to <see cref="AgentInstanceStatus.Idle"/>.
    /// </summary>
    public void FinishWork()
    {
        CurrentTaskId = null;
        Status = AgentInstanceStatus.Idle;
    }

    /// <summary>
    /// Marks this agent as <see cref="AgentInstanceStatus.Blocked"/>.
    /// </summary>
    public void Block()
    {
        Status = AgentInstanceStatus.Blocked;
    }

    /// <summary>
    /// Marks this agent as <see cref="AgentInstanceStatus.Finished"/>. The agent will not receive new tasks.
    /// </summary>
    public void MarkFinished()
    {
        Status = AgentInstanceStatus.Finished;
    }
}
