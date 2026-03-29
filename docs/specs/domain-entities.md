---
category: specs
title: "Domain Entities"
branch: "domain-entities"
status: ready
date: "2026-03-29"
related_domain: [Project, UseCase, URS, SRS, WorkTask, TaskAttempt, RevisionGate, AgentSlot, AgentInstance]
related_adr: []
---

# Feature Spec — Domain Entities

<!-- Reference this file in the implementation agent with: implement @docs/specs/domain-entities.md -->

---

## Context

This is the first implementation step for ChaosForge. The Domain layer entities form the
foundation on which all other layers are built. Without them, no Application use cases,
Infrastructure persistence, or API endpoints can be implemented. This scope is intentionally
narrow: base class, 9 core entities, supporting enumerations, and a domain exception — nothing
more.

---

## Domain Impact

- New or modified entity: `EntityBase<TId>` (base), `Project`, `UseCase`, `URS`, `SRS`,
  `WorkTask`, `TaskAttempt`, `RevisionGate`, `AgentSlot`, `AgentInstance`
- New domain event: none
- New interface: none
- New exception: `DomainException`
- New enumerations: `ProjectStatus`, `AgentRole`, `WorkTaskStatus`, `AttemptType`,
  `AttemptResult`, `RevisionGateType`, `RevisionGateStatus`, `RevisionGateAction`,
  `AgentInstanceStatus`

---

## Architecture Decisions

- `EntityBase<TId>` is `abstract`, generic, constrained to `where TId : notnull`. Contains
  `TId Id { get; private set; }` and `DateTime CreatedAt { get; private set; }`. Domain event
  collection is explicitly excluded — that belongs to the domain events spec.
- All entities are `sealed`; `EntityBase<TId>` is `abstract`.
- Entities must not use data annotations — Fluent API configuration is Infrastructure's
  responsibility.
- Each entity requires a `protected` parameterless constructor for EF Core, plus a `public`
  constructor with all required parameters calling `base(Guid.NewGuid())`.
- All property setters are `private` — state changes go exclusively through entity methods.
- `record` types are not used for entities (only for value objects). Entities are `class`.
- Invariants are enforced at the domain level by throwing `DomainException`.
- `DomainException` is a `sealed` custom exception class in the Domain layer.
- Every `public` member must have an XML doc comment.

---

## Implementation Scope — What must be done

### File structure

```
src/ChaosForge.Domain/
├── Common/
│   └── EntityBase.cs
├── Exceptions/
│   └── DomainException.cs
├── Enums/
│   ├── ProjectStatus.cs
│   ├── AgentRole.cs
│   ├── WorkTaskStatus.cs
│   ├── AttemptType.cs
│   ├── AttemptResult.cs
│   ├── RevisionGateType.cs
│   ├── RevisionGateStatus.cs
│   ├── RevisionGateAction.cs
│   └── AgentInstanceStatus.cs
└── Entities/
    ├── Project.cs
    ├── UseCase.cs
    ├── URS.cs
    ├── SRS.cs
    ├── WorkTask.cs
    ├── TaskAttempt.cs
    ├── RevisionGate.cs
    ├── AgentSlot.cs
    └── AgentInstance.cs
```

### Checklist

- [ ] Create `EntityBase<TId>` abstract class with `Id`, `CreatedAt`, and both constructors
- [ ] Create `DomainException` sealed class with `(string message)` and `(string message, Exception inner)` constructors
- [ ] Create all 9 enumerations (one file each)
- [ ] Create `Project` entity with `TransitionTo` status validation (linear sequence only)
- [ ] Create `UseCase` entity with `UpdatePriority` >= 0 guard
- [ ] Create `URS` entity with `ApplyHumanEdit` (both params required, non-whitespace)
- [ ] Create `SRS` entity with `ApplyHumanEdit` (same rules as URS)
- [ ] Create `WorkTask` entity with all 8 transition methods, each validating current status
- [ ] Create `TaskAttempt` entity with `Complete`, `Approve`, `Reject`; idempotency guard on resolve
- [ ] Create `RevisionGate` entity with `Accept`, `EditAndAccept`, `Reject`; already-resolved guard
- [ ] Create `AgentSlot` entity with `SingletonRoles` static set and `UpdateCount` guard
- [ ] Create `AgentInstance` entity with `StartWork`, `FinishWork`, `Block`, `MarkFinished`
- [ ] Verify `dotnet build` passes with zero warnings and zero errors

### Entity specifications

---

#### EntityBase

```
abstract class EntityBase<TId> where TId : notnull
  protected EntityBase()                  -- EF Core
  protected EntityBase(TId id)            -- sets Id, CreatedAt = DateTime.UtcNow
  TId      Id          { get; private set; }
  DateTime CreatedAt   { get; private set; }
```

---

#### DomainException

```
sealed class DomainException : Exception
  DomainException(string message)
  DomainException(string message, Exception inner)
```

---

#### Project

```
sealed class Project : EntityBase<Guid>
  string        Name         { get; private set; }
  string        Description  { get; private set; }
  ProjectStatus Status       { get; private set; }
  DateTime?     Deadline     { get; private set; }

  UpdateDescription(string description)
  TransitionTo(ProjectStatus newStatus)
    -- allowed sequence (forward only):
    -- Setup → RequirementsPhase → ArchitecturePhase → SprintPlanning → Development → Completed
    -- any other direction throws DomainException
```

---

#### UseCase

```
sealed class UseCase : EntityBase<Guid>
  Guid   ProjectId   { get; private set; }
  string Title       { get; private set; }
  string Description { get; private set; }
  int    Priority    { get; private set; }

  UpdatePriority(int priority)   -- priority >= 0, else DomainException
```

---

#### URS

```
sealed class URS : EntityBase<Guid>
  Guid    UseCaseId     { get; private set; }
  string  Title         { get; private set; }
  string  Description   { get; private set; }
  string? HumanEditNote { get; private set; }

  ApplyHumanEdit(string editedDescription, string note)
    -- both params: not null or whitespace, else DomainException
```

---

#### SRS

```
sealed class SRS : EntityBase<Guid>
  Guid    URSId                { get; private set; }
  string  Title                { get; private set; }
  string  TechnicalDescription { get; private set; }
  string? HumanEditNote        { get; private set; }

  ApplyHumanEdit(string editedDescription, string note)
    -- same rules as URS
```

---

#### WorkTask

```
sealed class WorkTask : EntityBase<Guid>
  Guid           SRSId       { get; private set; }
  Guid?          SprintId    { get; private set; }
  string         Title       { get; private set; }
  string         Description { get; private set; }
  WorkTaskStatus Status      { get; private set; }
  int            StoryPoints { get; private set; }

  AssignToSprint(Guid sprintId)  -- must be Backlog, sets SprintId
  Start()                        -- Backlog → InProgress; SprintId must be set
  SendToReview()                 -- InProgress → InReview
  Approve()                      -- InReview → InTesting
  SendToTesting()                -- alias for Approve, same transition
  PassTesting()                  -- InTesting → InDocumentation
  Complete()                     -- InDocumentation → Done
  Reject()                       -- InReview → Backlog  OR  InTesting → Backlog; resets SprintId = null
    -- any invalid current status throws DomainException
```

---

#### TaskAttempt

```
sealed class TaskAttempt : EntityBase<Guid>
  Guid          WorkTaskId       { get; private set; }
  Guid          AgentInstanceId  { get; private set; }
  AttemptType   Type             { get; private set; }
  string        Output           { get; private set; }
  string?       ReviewNote       { get; private set; }
  string?       TestNote         { get; private set; }
  AttemptResult Result           { get; private set; }   -- initialized to Pending
  DateTime      StartedAt        { get; private set; }   -- initialized to UtcNow
  DateTime?     CompletedAt      { get; private set; }

  Complete(string output)    -- output not null/whitespace; sets Output, CompletedAt = UtcNow
  Approve()                  -- sets Result = Approved; throws if already resolved
  Reject(string note)        -- note not null/whitespace; sets ReviewNote or TestNote based on Type
                             -- sets Result = Rejected; throws if already resolved
```

---

#### RevisionGate

```
sealed class RevisionGate : EntityBase<Guid>
  Guid                ProjectId         { get; private set; }
  RevisionGateType    Type              { get; private set; }
  RevisionGateStatus  Status            { get; private set; }   -- initialized to Open
  string              AgentOutput       { get; private set; }
  string?             HumanEditedOutput { get; private set; }
  string?             RejectionReason   { get; private set; }
  RevisionGateAction? Action            { get; private set; }
  DateTime?           ResolvedAt        { get; private set; }

  Accept()                           -- Action=Accept, Status=Resolved, ResolvedAt=UtcNow
  EditAndAccept(string editedOutput) -- editedOutput not null/whitespace
                                     -- sets HumanEditedOutput, Action=EditAndAccept, Status=Resolved, ResolvedAt=UtcNow
  Reject(string reason)              -- reason not null/whitespace
                                     -- sets RejectionReason, Action=Reject, Status=Resolved, ResolvedAt=UtcNow
  -- all three throw DomainException if Status is already Resolved
```

---

#### AgentSlot

```
sealed class AgentSlot : EntityBase<Guid>
  Guid      ProjectId { get; private set; }
  AgentRole Role      { get; private set; }
  int       Count     { get; private set; }

  public static readonly IReadOnlySet<AgentRole> SingletonRoles
    -- contains: BusinessAnalyst, Architect, ScrumMaster

  UpdateCount(int count)
    -- count >= 1, else DomainException
    -- if Role is in SingletonRoles and count > 1, DomainException
```

---

#### AgentInstance

```
sealed class AgentInstance : EntityBase<Guid>
  Guid                ProjectId     { get; private set; }
  AgentRole           Role          { get; private set; }
  string              PersonaName   { get; private set; }
  AgentInstanceStatus Status        { get; private set; }   -- initialized to Idle
  Guid?               CurrentTaskId { get; private set; }

  StartWork(Guid taskId)   -- sets CurrentTaskId, Status=Working; throws if already Working
  FinishWork()             -- clears CurrentTaskId, Status=Idle
  Block()                  -- Status=Blocked
  MarkFinished()           -- Status=Finished
```

---

## Out of Scope — What must NOT be done

- Do not create repository interfaces — separate spec
- Do not create domain event classes — separate spec
- Do not introduce `IDomainEventDispatcher` or any event infrastructure
- Do not write EF Core configuration — belongs to Infrastructure layer
- Do not write unit tests in this pass — separate spec
- Do not modify any other project (`Application`, `Infrastructure`, `API`)
- Do not add any NuGet package to the Domain project

---

## Test Expectations

- Unit tests required for: _(next spec — not part of this scope)_
- Edge cases to cover: _(next spec — not part of this scope)_

---

## Open Questions

- None. All decisions are derivable from `domain-model.md` and `architecture.md`.
