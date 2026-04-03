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
/// Records a single agent attempt at completing a work task step (implementation, review, testing, or documentation).
/// </summary>
public sealed class TaskAttempt : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private TaskAttempt()
    {
        Output = string.Empty;
    }

    /// <summary>
    /// Initializes a new task attempt in the <see cref="AttemptResult.Pending"/> state.
    /// </summary>
    /// <param name="workTaskId">The identifier of the work task being attempted.</param>
    /// <param name="agentInstanceId">The identifier of the agent making the attempt.</param>
    /// <param name="type">The type of work being attempted.</param>
    public TaskAttempt(Guid workTaskId, Guid agentInstanceId, AttemptType type) : base(Guid.NewGuid())
    {
        WorkTaskId = workTaskId;
        AgentInstanceId = agentInstanceId;
        Type = type;
        Output = string.Empty;
        Result = AttemptResult.Pending;
        StartedAt = DateTime.UtcNow;
    }

    /// <summary>Gets the identifier of the work task being attempted.</summary>
    public Guid WorkTaskId { get; private set; }

    /// <summary>Gets the identifier of the agent that made this attempt.</summary>
    public Guid AgentInstanceId { get; private set; }

    /// <summary>Gets the type of work performed in this attempt.</summary>
    public AttemptType Type { get; private set; }

    /// <summary>Gets the agent's output for this attempt.</summary>
    public string Output { get; private set; }

    /// <summary>Gets the reviewer's note for this attempt, if rejected during review.</summary>
    public string? ReviewNote { get; private set; }

    /// <summary>Gets the tester's note for this attempt, if rejected during testing.</summary>
    public string? TestNote { get; private set; }

    /// <summary>Gets the result of this attempt. Initialized to <see cref="AttemptResult.Pending"/>.</summary>
    public AttemptResult Result { get; private set; }

    /// <summary>Gets the UTC timestamp when this attempt was started.</summary>
    public DateTime StartedAt { get; private set; }

    /// <summary>Gets the UTC timestamp when this attempt was completed, if it has been completed.</summary>
    public DateTime? CompletedAt { get; private set; }

    /// <summary>
    /// Records the agent's output and marks the attempt as complete.
    /// </summary>
    /// <param name="output">The agent's output. Must not be null or whitespace.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="output"/> is null or whitespace.</exception>
    public void Complete(string output)
    {
        if (CompletedAt is not null)
        {
            throw new DomainException("Attempt has already been completed.");
        }

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new DomainException($"{nameof(output)} must not be null or whitespace.");
        }

        Output = output;
        CompletedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Approves this attempt, setting the result to <see cref="AttemptResult.Approved"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the attempt has already been resolved.</exception>
    public void Approve()
    {
        EnsureNotResolved();

        Result = AttemptResult.Approved;
    }

    /// <summary>
    /// Rejects this attempt with a review or test note depending on the attempt type.
    /// </summary>
    /// <param name="note">The rejection note. Must not be null or whitespace.</param>
    /// <exception cref="DomainException">Thrown when <paramref name="note"/> is null or whitespace, or the attempt is already resolved.</exception>
    public void Reject(string note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            throw new DomainException($"{nameof(note)} must not be null or whitespace.");
        }

        EnsureNotResolved();

        switch (Type)
        {
            case AttemptType.Review:
                ReviewNote = note;
                break;
            case AttemptType.Testing:
                TestNote = note;
                break;
            default:
                throw new DomainException($"Attempt type {Type} does not support rejection notes.");
        }

        Result = AttemptResult.Rejected;
    }

    private void EnsureNotResolved()
    {
        if (Result is not AttemptResult.Pending)
        {
            throw new DomainException($"Attempt has already been resolved with result {Result}.");
        }
    }
}
