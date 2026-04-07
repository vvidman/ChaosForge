---
category: specs
title: "AgentSlot, AgentInstance and TaskAttempt Commands"
branch: "agent-att-cmd"
status: ready
date: "2026-04-07"
related_domain: [AgentSlot, AgentInstance, TaskAttempt]
related_adr: []
---

# Feature Spec — AgentSlot, AgentInstance and TaskAttempt Commands

<!-- Reference this file in the implementation agent with: implement @docs/specs/16-agent-attempt-commands.md -->

---

## Context

The orchestrator needs to provision agents (AgentSlot), spin up agent instances
(AgentInstance), and record attempt lifecycle (TaskAttempt) through the Application layer.
Without these commands, the agent pipeline cannot interact with the domain through the
proper CQRS boundary. Follows the command handler pattern from specs 05, 06, and 15.
Depends on: specs 04 (pipeline), 02 (repository interfaces).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same conventions as previous command specs.
- Handlers in `Application/AgentSlots/Commands/`, `Application/AgentInstances/Commands/`,
  `Application/TaskAttempts/Commands/`.
- Return type for all commands: `Result` (non-generic).
- All domain method calls are wrapped in `try/catch (DomainException ex)` → `Result.Failure`.
- `CreateTaskAttemptCommand` returns `Result<Guid>` — the caller (orchestrator) needs the
  new attempt's Id to track it. This is the only command in this spec returning a typed Result.
- `AgentInstance` status transitions (`StartWork`, `FinishWork`, `Block`, `MarkFinished`)
  each get their own command — they represent distinct orchestrator actions.

---

## Implementation Scope — What must be done

### AgentSlot Commands

- [ ] `CreateAgentSlotCommand(Guid ProjectId, AgentRole Role, int Count)`
  + validator (ProjectId not empty, Count >= 1)
  + handler: `new AgentSlot(...)` → `AddAsync` → `SaveChangesAsync`

- [ ] `UpdateAgentSlotCountCommand(Guid AgentSlotId, int Count)`
  + validator (AgentSlotId not empty, Count >= 1)
  + handler: `GetByIdAsync` → not-found check → `UpdateCount(Count)` → `SaveChangesAsync`

### AgentInstance Commands

- [ ] `CreateAgentInstanceCommand(Guid ProjectId, AgentRole Role, string PersonaName)`
  + validator (ProjectId not empty, PersonaName not empty)
  + handler: `new AgentInstance(...)` → `AddAsync` → `SaveChangesAsync`

- [ ] `StartAgentWorkCommand(Guid AgentInstanceId, Guid TaskId)`
  + validator (both not empty)
  + handler: `GetByIdAsync` → not-found check → `StartWork(TaskId)` → `SaveChangesAsync`

- [ ] `FinishAgentWorkCommand(Guid AgentInstanceId)`
  + validator (not empty)
  + handler: `GetByIdAsync` → not-found check → `FinishWork()` → `SaveChangesAsync`

- [ ] `BlockAgentCommand(Guid AgentInstanceId)`
  + validator (not empty)
  + handler: `GetByIdAsync` → not-found check → `Block()` → `SaveChangesAsync`

- [ ] `MarkAgentFinishedCommand(Guid AgentInstanceId)`
  + validator (not empty)
  + handler: `GetByIdAsync` → not-found check → `MarkFinished()` → `SaveChangesAsync`

### TaskAttempt Commands

- [ ] `CreateTaskAttemptCommand(Guid WorkTaskId, Guid AgentInstanceId, AttemptType Type)`
  → `Result<Guid>` (returns the new attempt Id)
  + validator (WorkTaskId not empty, AgentInstanceId not empty)
  + handler: `new TaskAttempt(...)` → `AddAsync` → `SaveChangesAsync`
  → return `Result<Guid>.Success(attempt.Id)`

- [ ] `CompleteTaskAttemptCommand(Guid TaskAttemptId, string Output)`
  + validator (TaskAttemptId not empty, Output not empty)
  + handler: `GetByIdAsync` → not-found check → `Complete(Output)` → `SaveChangesAsync`

- [ ] `ApproveTaskAttemptCommand(Guid TaskAttemptId)`
  + validator (not empty)
  + handler: `GetByIdAsync` → not-found check → `Approve()` → `SaveChangesAsync`

- [ ] `RejectTaskAttemptCommand(Guid TaskAttemptId, string Note)`
  + validator (TaskAttemptId not empty, Note not empty)
  + handler: `GetByIdAsync` → not-found check → `Reject(Note)` → `SaveChangesAsync`

- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add API endpoints in this spec — that is spec 18
- Do not dispatch domain events — that is spec 19
- Do not implement the orchestration logic itself — that is a future spec

---

## Test Expectations

- Unit tests required for:
  - `CreateTaskAttemptCommand` handler: returns `Result<Guid>.Success` with the correct Id
  - `StartAgentWorkCommand` handler: happy path, not-found, DomainException (already working)
  - `CompleteTaskAttemptCommand` handler: happy path, not-found, DomainException (already completed)
  - Each remaining handler: happy path and not-found path
- Edge cases to cover: `RejectTaskAttemptCommand` with whitespace note → DomainException →
  handler returns Failure

---

## Open Questions

- None.
