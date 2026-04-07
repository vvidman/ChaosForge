---
category: specs
title: "Remaining API Endpoints"
branch: "api-endpoints"
status: ready
date: "2026-04-07"
related_domain: [UseCase, URS, SRS, WorkTask, RevisionGate, AgentSlot, AgentInstance, TaskAttempt]
related_adr: []
---

# Feature Spec — Remaining API Endpoints

<!-- Reference this file in the implementation agent with: implement @docs/specs/17-remaining-api-endpoints.md -->

---

## Context

Specs 11–16 produced query and command handlers for all aggregates, but only the Project
endpoints are exposed via HTTP. This spec wires every handler to a Minimal API endpoint,
completing the HTTP surface of the application. After this spec, the full Application layer
is reachable from outside. Depends on: specs 11–16.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- One `*Endpoints.cs` file per aggregate, following the pattern of `ProjectEndpoints.cs`.
- All files live in `src/ChaosForge.API/Endpoints/`.
- Each file contains one `MapXxxEndpoints(this IEndpointRouteBuilder app)` extension method.
- All extension methods are called in `Program.cs` after `app.MapProjectEndpoints()`.
- DTOs (request bodies) are defined in the same file as the endpoints — do not reuse
  Application layer DTOs as HTTP request bodies.
- Query results are returned as `Results.Ok(dto)` — the Application DTO is the response body.
- Command success returns `Results.Ok()` (non-generic), except `CreateTaskAttemptCommand`
  which returns `Results.Ok(new { id = result.Value })`.
- `Result.IsSuccess == false` → `Results.BadRequest(new { error = result.Error })`.
- No authentication, no CORS, no versioning in this spec.
- Route conventions:
  - `GET    /api/{resource}`              — list / get all
  - `GET    /api/{resource}/{id}`         — get by id
  - `POST   /api/{resource}`             — create
  - `PATCH  /api/{resource}/{id}/...`    — targeted mutation
  - `POST   /api/{resource}/{id}/...`    — state transition

---

## Implementation Scope — What must be done

### UseCaseEndpoints — `/api/usecases`
- [ ] `GET  /api/usecases/by-project/{projectId}` → `GetUseCasesByProjectIdQuery`
- [ ] `GET  /api/usecases/{id}`                   → `GetUseCaseByIdQuery`
- [ ] `POST /api/usecases`                         → `CreateUseCaseCommand`
- [ ] `PATCH /api/usecases/{id}/priority`          → `UpdateUseCasePriorityCommand`

### URSEndpoints — `/api/urs`
- [ ] `GET  /api/urs/by-usecase/{useCaseId}` → `GetURSsByUseCaseIdQuery`
- [ ] `GET  /api/urs/{id}`                   → `GetURSByIdQuery`
- [ ] `POST /api/urs`                         → `CreateURSCommand`
- [ ] `PATCH /api/urs/{id}/human-edit`        → `ApplyHumanEditToURSCommand`

### SRSEndpoints — `/api/srs`
- [ ] `GET  /api/srs/by-urs/{ursId}` → `GetSRSsByURSIdQuery`
- [ ] `GET  /api/srs/{id}`           → `GetSRSByIdQuery`
- [ ] `POST /api/srs`                 → `CreateSRSCommand`
- [ ] `PATCH /api/srs/{id}/human-edit` → `ApplyHumanEditToSRSCommand`

### WorkTaskEndpoints — `/api/worktasks`
- [ ] `GET  /api/worktasks/{id}`                           → `GetWorkTaskByIdQuery`
- [ ] `GET  /api/worktasks/by-srs/{srsId}`                → `GetWorkTasksBySRSIdQuery`
- [ ] `GET  /api/worktasks/by-sprint/{sprintId}`          → `GetWorkTasksBySprintIdQuery`
- [ ] `GET  /api/worktasks/by-status/{status}`            → `GetWorkTasksByStatusQuery`
- [ ] `POST /api/worktasks`                                → `CreateWorkTaskCommand`
- [ ] `POST /api/worktasks/{id}/assign-sprint`            → `AssignWorkTaskToSprintCommand`
- [ ] `POST /api/worktasks/{id}/start`                    → `StartWorkTaskCommand`
- [ ] `POST /api/worktasks/{id}/send-to-review`           → `SendWorkTaskToReviewCommand`
- [ ] `POST /api/worktasks/{id}/approve`                  → `ApproveWorkTaskCommand`
- [ ] `POST /api/worktasks/{id}/pass-testing`             → `PassWorkTaskTestingCommand`
- [ ] `POST /api/worktasks/{id}/complete`                 → `CompleteWorkTaskCommand`
- [ ] `POST /api/worktasks/{id}/reject`                   → `RejectWorkTaskCommand`

### RevisionGateEndpoints — `/api/revision-gates`
- [ ] `GET  /api/revision-gates/{id}`                         → `GetRevisionGateByIdQuery`
- [ ] `GET  /api/revision-gates/by-project/{projectId}`      → `GetRevisionGatesByProjectIdQuery`
- [ ] `GET  /api/revision-gates/open/by-project/{projectId}` → `GetOpenRevisionGateQuery`
- [ ] `POST /api/revision-gates`                              → `OpenRevisionGateCommand`
- [ ] `POST /api/revision-gates/{id}/accept`                  → `AcceptRevisionGateCommand`
- [ ] `POST /api/revision-gates/{id}/edit-and-accept`         → `EditAndAcceptRevisionGateCommand`
- [ ] `POST /api/revision-gates/{id}/reject`                  → `RejectRevisionGateCommand`

### AgentSlotEndpoints — `/api/agent-slots`
- [ ] `GET  /api/agent-slots/by-project/{projectId}` → `GetAgentSlotsByProjectIdQuery`
- [ ] `POST /api/agent-slots`                          → `CreateAgentSlotCommand`
- [ ] `PATCH /api/agent-slots/{id}/count`              → `UpdateAgentSlotCountCommand`

### AgentInstanceEndpoints — `/api/agent-instances`
- [ ] `GET  /api/agent-instances/{id}`                     → `GetAgentInstanceByIdQuery`
- [ ] `GET  /api/agent-instances/by-project/{projectId}`  → `GetAgentInstancesByProjectIdQuery`
- [ ] `GET  /api/agent-instances/by-status/{status}`      → `GetAgentInstancesByStatusQuery`
- [ ] `POST /api/agent-instances`                          → `CreateAgentInstanceCommand`
- [ ] `POST /api/agent-instances/{id}/start-work`         → `StartAgentWorkCommand`
- [ ] `POST /api/agent-instances/{id}/finish-work`        → `FinishAgentWorkCommand`
- [ ] `POST /api/agent-instances/{id}/block`              → `BlockAgentCommand`
- [ ] `POST /api/agent-instances/{id}/mark-finished`      → `MarkAgentFinishedCommand`

### TaskAttemptEndpoints — `/api/task-attempts`
- [ ] `GET  /api/task-attempts/{id}`                       → `GetTaskAttemptByIdQuery`
- [ ] `GET  /api/task-attempts/by-task/{workTaskId}`      → `GetTaskAttemptsByWorkTaskIdQuery`
- [ ] `POST /api/task-attempts`                            → `CreateTaskAttemptCommand`
  → response: `Results.Ok(new { id = result.Value })`
- [ ] `POST /api/task-attempts/{id}/complete`             → `CompleteTaskAttemptCommand`
- [ ] `POST /api/task-attempts/{id}/approve`              → `ApproveTaskAttemptCommand`
- [ ] `POST /api/task-attempts/{id}/reject`               → `RejectTaskAttemptCommand`

- [ ] Register all new `MapXxxEndpoints()` calls in `Program.cs`
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add authentication, CORS, or rate limiting
- Do not add response caching
- Do not modify existing `ProjectEndpoints.cs`

---

## Test Expectations

- Unit tests required for: none — endpoint wiring is verified by manual smoke tests
- Edge cases to cover: n/a

---

## Open Questions

- None.
