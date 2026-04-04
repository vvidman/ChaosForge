---
category: specs
title: "Domain Events"
branch: "domain-evts"
status: done
date: "2026-04-03"
related_domain: [Project, WorkTask, TaskAttempt, RevisionGate, AgentInstance]
related_adr: []
---

# Feature Spec — Domain Events

<!-- Reference this file in the implementation agent with: implement @docs/specs/03-domain-events.md -->

---

## Context

Domain events allow the Application layer to react to things that happened inside the
domain without coupling entities to infrastructure. This spec introduces the base
infrastructure (`IDomainEvent`, `IDomainEventDispatcher`) and the event classes for the
most important state transitions. `EntityBase<TId>` is extended to hold a domain event
collection.

---

## Domain Impact

- New or modified entity: `EntityBase<TId>` — add domain event collection and
  `AddDomainEvent` / `ClearDomainEvents` methods
- New domain event: `ProjectStatusChangedEvent`, `WorkTaskStatusChangedEvent`,
  `TaskAttemptCompletedEvent`, `TaskAttemptResolvedEvent`, `RevisionGateResolvedEvent`,
  `AgentInstanceStatusChangedEvent`
- New interface: `IDomainEvent`, `IDomainEventDispatcher`

---

## Architecture Decisions

- `IDomainEvent` is a marker interface (no members). Lives in `Domain/Events/`.
- All domain event classes are `sealed record` types — they are immutable value objects
  representing something that happened.
- `IDomainEventDispatcher` lives in `Domain/Events/`. It has one method:
  `Task DispatchAsync(IReadOnlyList<IDomainEvent> events, CancellationToken cancellationToken = default)`.
  The implementation lives in Infrastructure (out of scope here).
- `EntityBase<TId>` gains:
  - `private readonly List<IDomainEvent> _domainEvents = []`
  - `public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly()`
  - `protected void AddDomainEvent(IDomainEvent domainEvent)`
  - `public void ClearDomainEvents()`
- Domain events are raised inside entity methods (not from outside).
- Raising a domain event is always the last statement in the method that triggers it.
- Each event record carries only the data needed by a subscriber — no full entity references.

---

## Implementation Scope — What must be done

- [x] Create `IDomainEvent` marker interface in `Domain/Events/`
- [x] Create `IDomainEventDispatcher` in `Domain/Events/`
- [x] Extend `EntityBase<TId>` with domain event collection (see Architecture Decisions)
- [x] Create event records in `Domain/Events/`:

  `ProjectStatusChangedEvent`
  ```
  Guid ProjectId
  ProjectStatus OldStatus
  ProjectStatus NewStatus
  ```
  Raised in: `Project.TransitionTo()`

  `WorkTaskStatusChangedEvent`
  ```
  Guid WorkTaskId
  WorkTaskStatus OldStatus
  WorkTaskStatus NewStatus
  ```
  Raised in: `WorkTask.Start()`, `SendToReview()`, `Approve()`, `PassTesting()`,
  `Complete()`, `Reject()`

  `TaskAttemptCompletedEvent`
  ```
  Guid TaskAttemptId
  Guid WorkTaskId
  AttemptType Type
  ```
  Raised in: `TaskAttempt.Complete()`

  `TaskAttemptResolvedEvent`
  ```
  Guid TaskAttemptId
  Guid WorkTaskId
  AttemptResult Result
  ```
  Raised in: `TaskAttempt.Approve()` and `TaskAttempt.Reject()`

  `RevisionGateResolvedEvent`
  ```
  Guid RevisionGateId
  Guid ProjectId
  RevisionGateAction Action
  ```
  Raised in: `RevisionGate.Accept()`, `EditAndAccept()`, `Reject()`

  `AgentInstanceStatusChangedEvent`
  ```
  Guid AgentInstanceId
  AgentInstanceStatus OldStatus
  AgentInstanceStatus NewStatus
  ```
  Raised in: `AgentInstance.StartWork()`, `FinishWork()`, `Block()`, `MarkFinished()`

- [x] Update existing entity tests in `Domain.Tests` to assert that the correct domain event
  is added after each triggering method call (add assertions to existing test methods —
  do not create new test classes)
- [x] Run `dotnet build` and `dotnet test` — zero errors, zero failures

---

## Out of Scope — What must NOT be done

- Do not implement `IDomainEventDispatcher` — that is an Infrastructure concern
- Do not register anything in DI
- Do not dispatch events from entity constructors

---

## Test Expectations

- Unit tests required for: domain event assertions added to existing entity test methods
- Edge cases to cover: verify no duplicate events raised, verify `ClearDomainEvents` empties the list

---

## Open Questions

- None.
