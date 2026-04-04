---
category: specs
title: "Domain Unit Tests"
branch: "domain-tests"
status: done
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

- [x] `EntityBaseTests` — verify `Id` is set, `CreatedAt` is UTC and close to now
- [x] `ProjectTests`
  - [x] Constructor sets `Status = Setup`, `CreatedAt` is populated
  - [x] `TransitionTo` advances correctly through every sequential step
  - [x] `TransitionTo` throws `DomainException` for every backward or skip transition
  - [x] `TransitionTo` throws `DomainException` when already `Completed`
  - [x] `UpdateDescription` updates the value
  - [x] `UpdateDescription` throws `DomainException` for null, empty, and whitespace
- [x] `WorkTaskTests`
  - [x] Constructor sets `Status = Backlog`, validates title/description
  - [x] `AssignToSprint` sets `SprintId` when in `Backlog`
  - [x] `AssignToSprint` throws when not in `Backlog`
  - [x] `Start` transitions to `InProgress` when sprint is assigned
  - [x] `Start` throws when no sprint assigned
  - [x] `Start` throws when not in `Backlog`
  - [x] Full happy-path: `Backlog → InProgress → InReview → InTesting → InDocumentation → Done`
  - [x] `SendToTesting` is equivalent to `Approve`
  - [x] `Reject` from `InReview` resets `SprintId` to null and returns to `Backlog`
  - [x] `Reject` from `InTesting` resets `SprintId` to null and returns to `Backlog`
  - [x] `Reject` throws from every other status
- [x] `TaskAttemptTests`
  - [x] Constructor sets `Result = Pending`, `StartedAt` is populated, `Output` is empty
  - [x] `Complete` sets `Output` and `CompletedAt`
  - [x] `Complete` throws on null, empty, whitespace
  - [x] `Complete` throws when called a second time (idempotency guard)
  - [x] `Approve` sets `Result = Approved`
  - [x] `Approve` throws when already resolved (Approved or Rejected)
  - [x] `Reject` with `AttemptType.Review` sets `ReviewNote`
  - [x] `Reject` with `AttemptType.Testing` sets `TestNote`
  - [x] `Reject` with unsupported type throws `DomainException`
  - [x] `Reject` throws when already resolved
  - [x] `Reject` throws on null, empty, whitespace note
- [x] `RevisionGateTests`
  - [x] Constructor sets `Status = Open`
  - [x] `Accept` sets `Action`, `Status = Resolved`, `ResolvedAt`
  - [x] `EditAndAccept` sets `HumanEditedOutput`, `Action`, `Status = Resolved`
  - [x] `EditAndAccept` throws on null, empty, whitespace
  - [x] `Reject` sets `RejectionReason`, `Action`, `Status = Resolved`
  - [x] `Reject` throws on null, empty, whitespace
  - [x] All three resolution methods throw `DomainException` when gate is already resolved
- [x] `AgentSlotTests`
  - [x] `UpdateCount` accepts count >= 1 for non-singleton roles
  - [x] `UpdateCount` throws for count < 1
  - [x] `UpdateCount` throws for count > 1 on singleton roles
  - [x] `SingletonRoles` contains exactly `BusinessAnalyst`, `Architect`, `ScrumMaster`
- [x] `AgentInstanceTests`
  - [x] Constructor sets `Status = Idle`
  - [x] `StartWork` sets `CurrentTaskId` and `Status = Working`
  - [x] `StartWork` throws when already `Working`
  - [x] `FinishWork` clears `CurrentTaskId` and sets `Status = Idle`
  - [x] `Block` sets `Status = Blocked`
  - [x] `MarkFinished` sets `Status = Finished`
- [x] Run `dotnet test` — all tests must pass, zero skipped

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
