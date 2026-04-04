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
using ChaosForge.Domain.Events;
using ChaosForge.Domain.Exceptions;

namespace ChaosForge.Domain.Entities;

/// <summary>
/// Represents a human review checkpoint between major project phases.
/// The human can accept, edit and accept, or reject the agent's output.
/// </summary>
public sealed class RevisionGate : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private RevisionGate()
    {
        AgentOutput = string.Empty;
    }

    /// <summary>
    /// Initializes a new revision gate in the <see cref="RevisionGateStatus.Open"/> state.
    /// </summary>
    /// <param name="projectId">The identifier of the project this gate belongs to.</param>
    /// <param name="type">The phase this gate guards.</param>
    /// <param name="agentOutput">The output produced by the agent for human review.</param>
    public RevisionGate(Guid projectId, RevisionGateType type, string agentOutput) : base(Guid.NewGuid())
    {
        ProjectId = projectId;
        Type = type;
        AgentOutput = agentOutput;
        Status = RevisionGateStatus.Open;
    }

    /// <summary>Gets the identifier of the project this gate belongs to.</summary>
    public Guid ProjectId { get; private set; }

    /// <summary>Gets the phase this gate guards.</summary>
    public RevisionGateType Type { get; private set; }

    /// <summary>Gets the current status of this gate. Initialized to <see cref="RevisionGateStatus.Open"/>.</summary>
    public RevisionGateStatus Status { get; private set; }

    /// <summary>Gets the output produced by the agent for human review.</summary>
    public string AgentOutput { get; private set; }

    /// <summary>Gets the human-edited version of the agent output, if the human chose to edit before accepting.</summary>
    public string? HumanEditedOutput { get; private set; }

    /// <summary>Gets the reason the human rejected the agent output, if applicable.</summary>
    public string? RejectionReason { get; private set; }

    /// <summary>Gets the action taken by the human to resolve this gate, if resolved.</summary>
    public RevisionGateAction? Action { get; private set; }

    /// <summary>Gets the UTC timestamp when this gate was resolved, if it has been resolved.</summary>
    public DateTime? ResolvedAt { get; private set; }

    /// <summary>
    /// Accepts the agent output as-is, resolving the gate.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the gate is already resolved.</exception>
    public void Accept()
    {
        EnsureOpen();

        Action = RevisionGateAction.Accept;
        Status = RevisionGateStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        AddDomainEvent(new RevisionGateResolvedEvent(Id, ProjectId, RevisionGateAction.Accept));
    }

    /// <summary>
    /// Accepts the agent output after applying a human edit, resolving the gate.
    /// </summary>
    /// <param name="editedOutput">The human-edited version of the output. Must not be null or whitespace.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="editedOutput"/> is null or whitespace, or the gate is already resolved.</exception>
    public void EditAndAccept(string editedOutput)
    {
        if (string.IsNullOrWhiteSpace(editedOutput))
        {
            throw new DomainException($"{nameof(editedOutput)} must not be null or whitespace.");
        }

        EnsureOpen();

        HumanEditedOutput = editedOutput;
        Action = RevisionGateAction.EditAndAccept;
        Status = RevisionGateStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        AddDomainEvent(new RevisionGateResolvedEvent(Id, ProjectId, RevisionGateAction.EditAndAccept));
    }

    /// <summary>
    /// Rejects the agent output with a reason, resolving the gate and requiring the agent to redo the work.
    /// </summary>
    /// <param name="reason">The reason for rejection. Must not be null or whitespace.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="reason"/> is null or whitespace, or the gate is already resolved.</exception>
    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new DomainException($"{nameof(reason)} must not be null or whitespace.");
        }

        EnsureOpen();

        RejectionReason = reason;
        Action = RevisionGateAction.Reject;
        Status = RevisionGateStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        AddDomainEvent(new RevisionGateResolvedEvent(Id, ProjectId, RevisionGateAction.Reject));
    }

    private void EnsureOpen()
    {
        if (Status is not RevisionGateStatus.Open)
        {
            throw new DomainException("Revision gate has already been resolved.");
        }
    }
}
