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
/// Represents a unit of work derived from an SRS that flows through the team's workflow.
/// </summary>
public sealed class WorkTask : EntityBase<Guid>
{
    /// <summary>Parameterless constructor required by EF Core. Do not use directly.</summary>
    private WorkTask()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    /// <summary>
    /// Initializes a new work task in the <see cref="WorkTaskStatus.Backlog"/> state.
    /// </summary>
    /// <param name="srsId">The identifier of the SRS this task implements.</param>
    /// <param name="title">The short title of the task.</param>
    /// <param name="description">A detailed description of the work to be done.</param>
    /// <param name="storyPoints">The estimated complexity in story points.</param>
    public WorkTask(Guid srsId, string title, string description, int storyPoints) : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException($"{nameof(title)} must not be null or whitespace.");
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException($"{nameof(description)} must not be null or whitespace.");
        }

        SRSId = srsId;
        Title = title;
        Description = description;
        StoryPoints = storyPoints;
        Status = WorkTaskStatus.Backlog;
    }

    /// <summary>Gets the identifier of the SRS this task implements.</summary>
    public Guid SRSId { get; private set; }

    /// <summary>Gets the identifier of the sprint this task is assigned to, if any.</summary>
    public Guid? SprintId { get; private set; }

    /// <summary>Gets the short title of the task.</summary>
    public string Title { get; private set; }

    /// <summary>Gets the detailed description of the work to be done.</summary>
    public string Description { get; private set; }

    /// <summary>Gets the current workflow status of the task.</summary>
    public WorkTaskStatus Status { get; private set; }

    /// <summary>Gets the estimated complexity in story points.</summary>
    public int StoryPoints { get; private set; }

    /// <summary>
    /// Assigns this task to a sprint. Task must be in <see cref="WorkTaskStatus.Backlog"/>.
    /// </summary>
    /// <param name="sprintId">The identifier of the sprint.</param>
    /// <exception cref="DomainException">Thrown when the task is not in Backlog status.</exception>
    public void AssignToSprint(Guid sprintId)
    {
        if (Status is not WorkTaskStatus.Backlog)
        {
            throw new DomainException($"Cannot assign task to sprint: current status is {Status}, expected {WorkTaskStatus.Backlog}.");
        }

        SprintId = sprintId;
    }

    /// <summary>
    /// Moves the task from <see cref="WorkTaskStatus.Backlog"/> to <see cref="WorkTaskStatus.InProgress"/>.
    /// A sprint must be assigned before starting.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not in Backlog or has no sprint assigned.</exception>
    public void Start()
    {
        if (Status is not WorkTaskStatus.Backlog)
        {
            throw new DomainException($"Cannot start task: current status is {Status}, expected {WorkTaskStatus.Backlog}.");
        }

        if (SprintId is null)
        {
            throw new DomainException("Cannot start task: no sprint has been assigned.");
        }

        var oldStatus = Status;
        Status = WorkTaskStatus.InProgress;
        AddDomainEvent(new WorkTaskStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Moves the task from <see cref="WorkTaskStatus.InProgress"/> to <see cref="WorkTaskStatus.InReview"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not InProgress.</exception>
    public void SendToReview()
    {
        if (Status is not WorkTaskStatus.InProgress)
        {
            throw new DomainException($"Cannot send task to review: current status is {Status}, expected {WorkTaskStatus.InProgress}.");
        }

        var oldStatus = Status;
        Status = WorkTaskStatus.InReview;
        AddDomainEvent(new WorkTaskStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Moves the task from <see cref="WorkTaskStatus.InReview"/> to <see cref="WorkTaskStatus.InTesting"/>.
    /// Alias for <see cref="Approve"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not InReview.</exception>
    public void SendToTesting()
    {
        Approve();
    }

    /// <summary>
    /// Moves the task from <see cref="WorkTaskStatus.InReview"/> to <see cref="WorkTaskStatus.InTesting"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not InReview.</exception>
    public void Approve()
    {
        if (Status is not WorkTaskStatus.InReview)
        {
            throw new DomainException($"Cannot approve task: current status is {Status}, expected {WorkTaskStatus.InReview}.");
        }

        var oldStatus = Status;
        Status = WorkTaskStatus.InTesting;
        AddDomainEvent(new WorkTaskStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Moves the task from <see cref="WorkTaskStatus.InTesting"/> to <see cref="WorkTaskStatus.InDocumentation"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not InTesting.</exception>
    public void PassTesting()
    {
        if (Status is not WorkTaskStatus.InTesting)
        {
            throw new DomainException($"Cannot pass testing: current status is {Status}, expected {WorkTaskStatus.InTesting}.");
        }

        var oldStatus = Status;
        Status = WorkTaskStatus.InDocumentation;
        AddDomainEvent(new WorkTaskStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Moves the task from <see cref="WorkTaskStatus.InDocumentation"/> to <see cref="WorkTaskStatus.Done"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not InDocumentation.</exception>
    public void Complete()
    {
        if (Status is not WorkTaskStatus.InDocumentation)
        {
            throw new DomainException($"Cannot complete task: current status is {Status}, expected {WorkTaskStatus.InDocumentation}.");
        }

        var oldStatus = Status;
        Status = WorkTaskStatus.Done;
        AddDomainEvent(new WorkTaskStatusChangedEvent(Id, oldStatus, Status));
    }

    /// <summary>
    /// Rejects the task, returning it to <see cref="WorkTaskStatus.Backlog"/> and clearing the sprint assignment.
    /// Valid from <see cref="WorkTaskStatus.InReview"/> or <see cref="WorkTaskStatus.InTesting"/>.
    /// </summary>
    /// <exception cref="DomainException">Thrown when the task is not in a rejectable status.</exception>
    public void Reject()
    {
        if (Status is not WorkTaskStatus.InReview and not WorkTaskStatus.InTesting)
        {
            throw new DomainException($"Cannot reject task: current status is {Status}. Must be {WorkTaskStatus.InReview} or {WorkTaskStatus.InTesting}.");
        }

        var oldStatus = Status;
        Status = WorkTaskStatus.Backlog;
        SprintId = null;
        AddDomainEvent(new WorkTaskStatusChangedEvent(Id, oldStatus, Status));
    }
}
