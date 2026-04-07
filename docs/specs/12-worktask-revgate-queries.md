---
category: specs
title: "WorkTask and RevisionGate Queries"
branch: "task-gate-qry"
status: done
date: "2026-04-07"
related_domain: [WorkTask, RevisionGate]
related_adr: []
---

# Feature Spec — WorkTask and RevisionGate Queries

<!-- Reference this file in the implementation agent with: implement @docs/specs/12-worktask-revgate-queries.md -->

---

## Context

WorkTask and RevisionGate have rich query needs — tasks can be filtered by SRS, sprint, or
status; gates can be listed per project or queried for the currently open one. These are
the two most query-heavy aggregates in the current scope and are grouped together because
their handler patterns are identical. Depends on spec 11 (query pattern established).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same conventions as spec 11: DTOs, `internal sealed` handlers, no entity exposure outside
  Application, no `IUnitOfWork` in query handlers.
- All query handlers live in `Application/WorkTasks/Queries/` and
  `Application/RevisionGates/Queries/` respectively.
- List queries return `Result<IReadOnlyList<TDto>>.Success(...)` — never Failure on empty.
- Single-entity queries return `Result<TDto>.Failure("... not found.")` when missing.
- `WorkTaskDto` carries all fields including `SprintId` (nullable).
- `RevisionGateDto` carries all fields including nullable `HumanEditedOutput`,
  `RejectionReason`, `Action`, `ResolvedAt`.

---

## Implementation Scope — What must be done

### WorkTask DTOs and Queries

- [x] Create `WorkTaskDto` record:
  ```
  Guid Id, Guid SRSId, Guid? SprintId, string Title, string Description,
  WorkTaskStatus Status, int StoryPoints, DateTime CreatedAt
  ```
- [x] `GetWorkTaskByIdQuery(Guid WorkTaskId)` → `Result<WorkTaskDto>`
  - Handler: `GetByIdAsync` → map → Success, or Failure if null
- [x] `GetWorkTasksBySRSIdQuery(Guid SRSId)` → `Result<IReadOnlyList<WorkTaskDto>>`
  - Handler: `GetBySRSIdAsync` → map all
- [x] `GetWorkTasksBySprintIdQuery(Guid SprintId)` → `Result<IReadOnlyList<WorkTaskDto>>`
  - Handler: `GetBySprintIdAsync` → map all
- [x] `GetWorkTasksByStatusQuery(WorkTaskStatus Status)` → `Result<IReadOnlyList<WorkTaskDto>>`
  - Handler: `GetByStatusAsync` → map all

### RevisionGate DTOs and Queries

- [x] Create `RevisionGateDto` record:
  ```
  Guid Id, Guid ProjectId, RevisionGateType Type, RevisionGateStatus Status,
  string AgentOutput, string? HumanEditedOutput, string? RejectionReason,
  RevisionGateAction? Action, DateTime? ResolvedAt, DateTime CreatedAt
  ```
- [x] `GetRevisionGateByIdQuery(Guid RevisionGateId)` → `Result<RevisionGateDto>`
  - Handler: `GetByIdAsync` → map → Success, or Failure if null
- [x] `GetRevisionGatesByProjectIdQuery(Guid ProjectId)` → `Result<IReadOnlyList<RevisionGateDto>>`
  - Handler: `GetByProjectIdAsync` → map all
- [x] `GetOpenRevisionGateQuery(Guid ProjectId)` → `Result<RevisionGateDto>`
  - Handler: `GetOpenByProjectIdAsync` → Success if found, Failure("No open revision gate found for this project.") if null

- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add API endpoints in this spec — that is spec 18
- Do not add filtering beyond what the existing repository methods support
- Do not return attempt history from WorkTask queries — that is spec 14

---

## Test Expectations

- Unit tests required for:
  - Each query handler: not-found path (where applicable), happy path with correct field mapping
  - `GetOpenRevisionGateQuery`: returns Failure when no open gate exists
- Edge cases to cover: `GetWorkTasksByStatusQuery` with no matching tasks returns empty list,
  not Failure

---

## Open Questions

- None.
