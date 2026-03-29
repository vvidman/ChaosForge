---
category: domain
area: "Work Execution"
last_updated: "2026-03-29"
related_adr: [ADR-006]
---

# Work Execution

## Overview
Work execution covers everything from sprint planning to task completion. The ScrumMaster
assigns WorkTasks to Sprints; agent workers then pick up tasks and execute development,
review, testing, and documentation cycles. Each cycle produces a `TaskAttempt`. Rejection
at any stage sends the task back to `Backlog` with notes that feed the next attempt.

## Key Concepts

### Sprint
A time-boxed container for WorkTasks, planned by the ScrumMaster.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| ProjectId | Guid | |
| SprintNumber | int | sequential, 1-based |
| StartDate | DateTime? | set when sprint is started |
| EndDate | DateTime? | target end, used for SM planning |
| IsActive | bool | only one sprint active at a time |

### WorkTask
A unit of development work derived from an SRS item.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| SRSId | Guid | |
| SprintId | Guid? | null until sprint planning assigns it |
| Title | string | |
| Description | string | |
| Status | WorkTaskStatus | |
| StoryPoints | int | estimated by Architect at creation |

### WorkTaskStatus state machine
```
Backlog → InProgress → InReview → InTesting → InDocumentation → Done
                           ↓            ↓
                        Backlog      Backlog    (on Reviewer or Tester rejection)
```

A task returns to `Backlog` with rejection notes; the next `TaskAttempt` picks up those notes
as prompt context.

### TaskAttempt
One attempt by one agent instance on one task. Immutable once `Result` is set.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| WorkTaskId | Guid | |
| AgentInstanceId | Guid | which agent instance ran this attempt |
| Type | AttemptType | Development / Review / Testing / Documentation |
| Output | string | raw Markdown or JSON from LLM |
| ReviewNote | string? | set by Reviewer on rejection |
| TestNote | string? | set by Tester on rejection |
| Result | AttemptResult | Pending / Approved / Rejected |
| StartedAt | DateTime | |
| CompletedAt | DateTime? | null while Pending |

### AttemptType
`Development | Review | Testing | Documentation`
— maps to the agent role that produces the attempt.

### AttemptResult
`Pending | Approved | Rejected`

### Attempt–prompt injection rule
When a new attempt is created for a task that has a previous `Rejected` attempt,
the new attempt's prompt receives: previous attempt `Output` + relevant rejection note
(`ReviewNote` or `TestNote`). This is the only context carried forward — full history
is not injected.

## Business Rules
- A WorkTask can have at most one `TaskAttempt` with `Result = Pending` at a time
- A WorkTask can only move to `InProgress` if its Sprint is active (`IsActive = true`)
- Only one Sprint may be active at a time within a project
- `CompletedAt` must be set when `Result` transitions from `Pending` to `Approved` or `Rejected`
- `ReviewNote` is required on a Review attempt with `Result = Rejected`
- `TestNote` is required on a Testing attempt with `Result = Rejected`
- A task rejected from `InReview` or `InTesting` returns to `Backlog`; `SprintId` is retained

## Boundaries
**In scope:** Sprint, WorkTask, TaskAttempt entities; WorkTaskStatus state machine; attempt lifecycle; rejection note injection rule
**Out of scope:** how tasks are created (see `requirements-pipeline.md`), sprint planning decisions (see `agent-system.md`), revision gate for phase-level decisions (see `revision-gate.md`)

## Related Areas
- `requirements-pipeline.md` — SRS items that WorkTasks are derived from
- `agent-system.md` — agent instances that execute TaskAttempts
- `revision-gate.md` — AfterScrumMaster gate that precedes Development phase

## References
- See ADR-006 for TaskAttempt per-cycle design rationale
- See `agent-design.md` (architecture) for Developer, Tester, Reviewer, TechnicalWriter roles
