---
category: specs
title: "Development Loop Orchestration Handlers"
branch: "orch-devloop"
status: ready
date: "2026-04-12"
related_domain: [WorkTask, TaskAttempt, AgentInstance]
related_adr: [003-background-service-workers, 006-task-attempt-per-cycle]
---

# Feature Spec — Development Loop Orchestration Handlers

<!-- Reference this file in the implementation agent with: implement @docs/specs/27-orchestrator-dev-loop.md -->

---

## Context

The phase orchestrator (spec 26) handles phase transitions. But within the Development
phase, there is a second layer of orchestration: reacting to individual task lifecycle
events. When a task attempt is resolved (approved or rejected), something must update the
task status and handle the rejection note injection for the next attempt. This spec
implements the notification handlers for `TaskAttemptResolvedEvent` and
`WorkTaskStatusChangedEvent`. Depends on: spec 26 (orchestration pattern established).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same conventions as spec 26: `INotificationHandler<T>` in `Application/Orchestration/`.
- `TaskAttemptResolvedHandler` reacts to `TaskAttemptResolvedEvent`:
  - If `Result == Approved`: the task status transition is already handled by the agent
    worker (the worker called `ApproveWorkTaskCommand` etc.) — no action needed here.
    The handler is a no-op but exists for observability (log the approval).
  - If `Result == Rejected`: the rejection is also already applied by the worker (the
    worker currently always approves, but manual API rejections can still fire this event).
    Handler logs for observability. No additional state mutation needed — the task is
    already back in `Backlog` with rejection notes on the attempt.
- `WorkTaskStatusChangedHandler` reacts to `WorkTaskStatusChangedEvent`:
  - Logs the transition for observability.
  - When `NewStatus == Done`: check if all WorkTasks in the project are `Done`.
    If yes: mark the project as `Completed` via `TransitionProjectCommand`.
  - This is the automatic project completion trigger — when the last task is done,
    the project closes itself.
- All-tasks-done check: query `GetWorkTasksByStatusQuery` for `Backlog`, `InProgress`,
  `InReview`, `InTesting`, `InDocumentation`. If all return empty lists AND at least one
  task exists with `Done` status, transition to `Completed`.
- Guard: only attempt the completion check if `NewStatus == Done` and project is currently
  in `Development` phase.

---

## Implementation Scope — What must be done

- [ ] Create `Application/Orchestration/TaskAttemptResolvedHandler.cs`:
  - Implements `INotificationHandler<TaskAttemptResolvedEvent>`
  - Logs approval or rejection
  - No state mutation (see Architecture Decisions)

- [ ] Create `Application/Orchestration/WorkTaskStatusChangedHandler.cs`:
  - Implements `INotificationHandler<WorkTaskStatusChangedEvent>`
  - Logs the transition
  - On `NewStatus == Done`: fetch project, guard phase check, query all non-Done statuses,
    if all empty → `TransitionProjectCommand` to `Completed`

- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not re-implement task status transitions here — the agent workers and command handlers
  already handle them
- Do not implement retry scheduling here — the polling workers handle retries naturally
- Do not notify the frontend here — that is spec 30

---

## Test Expectations

- Unit tests required for:
  - `WorkTaskStatusChangedHandler`: last task transitions to Done, all other statuses empty →
    `TransitionProjectCommand(Completed)` sent
  - `WorkTaskStatusChangedHandler`: task transitions to Done but other tasks still in progress →
    no completion command sent
  - `WorkTaskStatusChangedHandler`: `NewStatus != Done` → no completion check, no command
  - `TaskAttemptResolvedHandler`: fires without throwing for both Approved and Rejected results
- Edge cases to cover: project already Completed when handler fires → `TransitionProjectCommand`
  will throw `DomainException` inside the command handler — this propagates up; the
  notification handler should catch and log it rather than crash the dispatcher chain

---

## Open Questions

- The all-tasks-done check is eventually consistent by design: there is a brief window
  between the last task completing and the project closing. This is acceptable for a
  single-process hobby tool.
