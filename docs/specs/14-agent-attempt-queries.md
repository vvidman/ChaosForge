---
category: specs
title: "AgentSlot, AgentInstance and TaskAttempt Queries"
branch: "agent-att-qry"
status: ready
date: "2026-04-07"
related_domain: [AgentSlot, AgentInstance, TaskAttempt]
related_adr: []
---

# Feature Spec — AgentSlot, AgentInstance and TaskAttempt Queries

<!-- Reference this file in the implementation agent with: implement @docs/specs/14-agent-attempt-queries.md -->

---

## Context

The agent monitoring panel and task history view both require read access to AgentSlot,
AgentInstance, and TaskAttempt data. These three are grouped because they share the same
query pattern and are all consumed by the same frontend dashboard context.
Depends on spec 11 (query pattern established).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same conventions as specs 11–13.
- Handlers live in `Application/AgentSlots/Queries/`, `Application/AgentInstances/Queries/`,
  `Application/TaskAttempts/Queries/`.
- `AgentInstanceDto` includes `CurrentTaskId` (nullable).
- `TaskAttemptDto` includes all fields: `ReviewNote`, `TestNote` (both nullable),
  `CompletedAt` (nullable).
- `GetTaskAttemptsByWorkTaskIdQuery` is the primary access pattern for attempt history.

---

## Implementation Scope — What must be done

### AgentSlot

- [ ] Create `AgentSlotDto` record:
  ```
  Guid Id, Guid ProjectId, AgentRole Role, int Count, DateTime CreatedAt
  ```
- [ ] `GetAgentSlotsByProjectIdQuery(Guid ProjectId)` → `Result<IReadOnlyList<AgentSlotDto>>`
  - Handler: `IAgentSlotRepository.GetByProjectIdAsync`

### AgentInstance

- [ ] Create `AgentInstanceDto` record:
  ```
  Guid Id, Guid ProjectId, AgentRole Role, string PersonaName,
  AgentInstanceStatus Status, Guid? CurrentTaskId, DateTime CreatedAt
  ```
- [ ] `GetAgentInstanceByIdQuery(Guid AgentInstanceId)` → `Result<AgentInstanceDto>`
- [ ] `GetAgentInstancesByProjectIdQuery(Guid ProjectId)` → `Result<IReadOnlyList<AgentInstanceDto>>`
  - Handler: `IAgentInstanceRepository.GetByProjectIdAsync`
- [ ] `GetAgentInstancesByStatusQuery(AgentInstanceStatus Status)` → `Result<IReadOnlyList<AgentInstanceDto>>`
  - Handler: `IAgentInstanceRepository.GetByStatusAsync`

### TaskAttempt

- [ ] Create `TaskAttemptDto` record:
  ```
  Guid Id, Guid WorkTaskId, Guid AgentInstanceId, AttemptType Type,
  string Output, string? ReviewNote, string? TestNote,
  AttemptResult Result, DateTime StartedAt, DateTime? CompletedAt, DateTime CreatedAt
  ```
- [ ] `GetTaskAttemptByIdQuery(Guid TaskAttemptId)` → `Result<TaskAttemptDto>`
- [ ] `GetTaskAttemptsByWorkTaskIdQuery(Guid WorkTaskId)` → `Result<IReadOnlyList<TaskAttemptDto>>`
  - Handler: `ITaskAttemptRepository.GetByWorkTaskIdAsync`

- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add API endpoints in this spec — that is spec 18
- Do not add AgentInstance commands in this spec — that is spec 16

---

## Test Expectations

- Unit tests required for:
  - `GetAgentInstanceByIdQuery`: not-found Failure, found maps all fields
  - `GetTaskAttemptsByWorkTaskIdQuery`: empty list on no attempts
  - `GetTaskAttemptByIdQuery`: not-found Failure
- Edge cases to cover: nullable fields (`CurrentTaskId`, `CompletedAt`, `ReviewNote`,
  `TestNote`) are null in DTO when entity field is null

---

## Open Questions

- None.
