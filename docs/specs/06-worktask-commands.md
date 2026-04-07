---
category: specs
title: "WorkTask Commands"
branch: "task-cmds"
status: done
date: "2026-04-03"
related_domain: [WorkTask, SRS]
related_adr: []
---

# Feature Spec — WorkTask Commands

<!-- Reference this file in the implementation agent with: implement @docs/specs/06-worktask-commands.md -->

---

## Context

Work tasks are the atomic unit of work flowing through the agent pipeline. This spec covers
all write operations on `WorkTask`: creation, sprint assignment, and all status transitions.
Depends on: Application Pipeline (spec 04), Repository Interfaces (spec 02).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same structural conventions as spec 05 (Project Commands).
- Commands live in `Application/WorkTasks/Commands/`.
- Each command, validator, and handler in the same file.
- Handlers are `internal sealed`.
- `SRSId` is not validated for existence in this spec — referential integrity is an
  Infrastructure concern enforced by the database.
- Return type for all commands: `Result` (non-generic).

---

## Implementation Scope — What must be done

- [x] `CreateWorkTaskCommand` + validator + handler
  ```
  Command: SRSId (Guid), Title (string), Description (string), StoryPoints (int)
  Validator: SRSId not empty, Title not empty, Description not empty, StoryPoints >= 1
  Handler: new WorkTask(...) → AddAsync → SaveChangesAsync
  ```
- [x] `AssignWorkTaskToSprintCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid), SprintId (Guid)
  Validator: both not empty
  Handler: GetByIdAsync → AssignToSprint(SprintId) → SaveChangesAsync
  ```
- [x] `StartWorkTaskCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid)
  Validator: not empty
  Handler: GetByIdAsync → Start() → SaveChangesAsync
  ```
- [x] `SendWorkTaskToReviewCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid)
  Handler: GetByIdAsync → SendToReview() → SaveChangesAsync
  ```
- [x] `ApproveWorkTaskCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid)
  Handler: GetByIdAsync → Approve() → SaveChangesAsync
  ```
- [x] `PassWorkTaskTestingCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid)
  Handler: GetByIdAsync → PassTesting() → SaveChangesAsync
  ```
- [x] `CompleteWorkTaskCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid)
  Handler: GetByIdAsync → Complete() → SaveChangesAsync
  ```
- [x] `RejectWorkTaskCommand` + validator + handler
  ```
  Command: WorkTaskId (Guid)
  Handler: GetByIdAsync → Reject() → SaveChangesAsync
  ```
- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement WorkTask queries (separate spec)
- Do not create sprint entities — `SprintId` is a plain `Guid` reference at this stage
- Do not dispatch domain events

---

## Test Expectations

- Unit tests required for:
  - Each handler: happy path, not-found, domain exception
  - `CreateWorkTaskCommand` validator: StoryPoints < 1 fails
- Edge cases to cover: `Start()` without sprint assigned (domain throws, handler returns Failure)

---

## Open Questions

- None.
