---
category: specs
title: "Domain Unit Tests"
branch: "domain-tests"
status: ready
date: "2026-04-03"
related_domain: [Project, UseCase, URS, SRS, WorkTask, TaskAttempt, RevisionGate, AgentSlot, AgentInstance]
related_adr: []
---

# Feature Spec — Domain Unit Tests

<!-- Reference this file in the implementation agent with: implement @docs/specs/01-domain-unit-tests.md -->

---

## Context

The domain entities are implemented and reviewed. Before moving to the Application layer,
the invariants and state-transition logic must be covered by unit tests. These tests act as
a regression harness — any future change to entity logic that breaks an invariant will be
caught immediately. All tests live in `ChaosForge.Domain.Tests`.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Test project: `tests/ChaosForge.Domain.Tests` (already scaffolded).
- Stack: xUnit, FluentAssertions 8, NSubstitute 5.
- One test class per entity, named `[EntityName]Tests`, in a folder structure mirroring
  `src/ChaosForge.Domain/Entities/`.
- Enumerations do not need dedicated test classes — they are covered indirectly via entity tests.
- No mocks needed — domain entities have zero external dependencies.
- Tests must be fully deterministic; no `DateTime.Now` or `Guid.NewGuid()` assertions on
  exact values — assert `>` some reference point or `!= Guid.Empty` instead.
- Follow Arrange / Act / Assert structure with a blank line separating each section.
- Test method naming: `MethodName_StateUnderTest_ExpectedBehavior`.

---

## Implementation Scope — What must be done

- [ ] `EntityBaseTests` — verify `Id` is set, `CreatedAt` is UTC and close to now
- [ ] `ProjectTests`
  - [ ] Constructor sets `Status = Setup`, `CreatedAt` is populated
  - [ ] `TransitionTo` advances correctly through every sequential step
  - [ ] `TransitionTo` throws `DomainException` for every backward or skip transition
  - [ ] `TransitionTo` throws `DomainException` when already `Completed`
  - [ ] `UpdateDescription` updates the value
  - [ ] `UpdateDescription` throws `DomainException` for null, empty, and whitespace
- [ ] `WorkTaskTests`
  - [ ] Constructor sets `Status = Backlog`, validates title/description
  - [ ] `AssignToSprint` sets `SprintId` when in `Backlog`
  - [ ] `AssignToSprint` throws when not in `Backlog`
  - [ ] `Start` transitions to `InProgress` when sprint is assigned
  - [ ] `Start` throws when no sprint assigned
  - [ ] `Start` throws when not in `Backlog`
  - [ ] Full happy-path: `Backlog → InProgress → InReview → InTesting → InDocumentation → Done`
  - [ ] `SendToTesting` is equivalent to `Approve`
  - [ ] `Reject` from `InReview` resets `SprintId` to null and returns to `Backlog`
  - [ ] `Reject` from `InTesting` resets `SprintId` to null and returns to `Backlog`
  - [ ] `Reject` throws from every other status
- [ ] `TaskAttemptTests`
  - [ ] Constructor sets `Result = Pending`, `StartedAt` is populated, `Output` is empty
  - [ ] `Complete` sets `Output` and `CompletedAt`
  - [ ] `Complete` throws on null, empty, whitespace
  - [ ] `Complete` throws when called a second time (idempotency guard)
  - [ ] `Approve` sets `Result = Approved`
  - [ ] `Approve` throws when already resolved (Approved or Rejected)
  - [ ] `Reject` with `AttemptType.Review` sets `ReviewNote`
  - [ ] `Reject` with `AttemptType.Testing` sets `TestNote`
  - [ ] `Reject` with unsupported type throws `DomainException`
  - [ ] `Reject` throws when already resolved
  - [ ] `Reject` throws on null, empty, whitespace note
- [ ] `RevisionGateTests`
  - [ ] Constructor sets `Status = Open`
  - [ ] `Accept` sets `Action`, `Status = Resolved`, `ResolvedAt`
  - [ ] `EditAndAccept` sets `HumanEditedOutput`, `Action`, `Status = Resolved`
  - [ ] `EditAndAccept` throws on null, empty, whitespace
  - [ ] `Reject` sets `RejectionReason`, `Action`, `Status = Resolved`
  - [ ] `Reject` throws on null, empty, whitespace
  - [ ] All three resolution methods throw `DomainException` when gate is already resolved
- [ ] `AgentSlotTests`
  - [ ] `UpdateCount` accepts count >= 1 for non-singleton roles
  - [ ] `UpdateCount` throws for count < 1
  - [ ] `UpdateCount` throws for count > 1 on singleton roles
  - [ ] `SingletonRoles` contains exactly `BusinessAnalyst`, `Architect`, `ScrumMaster`
- [ ] `AgentInstanceTests`
  - [ ] Constructor sets `Status = Idle`
  - [ ] `StartWork` sets `CurrentTaskId` and `Status = Working`
  - [ ] `StartWork` throws when already `Working`
  - [ ] `FinishWork` clears `CurrentTaskId` and sets `Status = Idle`
  - [ ] `Block` sets `Status = Blocked`
  - [ ] `MarkFinished` sets `Status = Finished`
- [ ] Run `dotnet test` — all tests must pass, zero skipped

---

## Out of Scope — What must NOT be done

- Do not test Application, Infrastructure, or API code
- Do not add integration tests in this pass
- Do not mock anything — domain entities have no dependencies
- Do not modify entity source files to make tests pass; if a test fails, report it

---

## Test Expectations

- Unit tests required for: all 9 entities as listed in the checklist above
- Edge cases to cover: invalid state transitions, double-resolution guards,
  whitespace/null input guards, singleton role count limit

---

## Open Questions

- None.
