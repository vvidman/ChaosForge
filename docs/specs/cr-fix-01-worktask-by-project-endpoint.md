---
category: specs
title: "CR Fix: WorkTask by-project API endpoint"
branch: "fix-worktask-by-project"
status: ready
date: "2026-04-25"
related_domain: [WorkTask]
related_adr: []
---

# Feature Spec — CR Fix: WorkTask by-project API endpoint

<!-- Reference this file in the implementation agent with: implement @docs/specs/cr-fix-01-worktask-by-project-endpoint.md -->

---

## Context

The frontend calls `GET /api/worktasks/by-project/:projectId` in both `SprintPage`
(KanbanBoard) and `HistoryPage`. This endpoint does not exist in the backend. The
domain repository interface `IWorkTaskRepository.GetByProjectIdAsync` exists and the
infrastructure implementation exists, but no Application query and no API endpoint
were ever wired up. The frontend will get 404 errors on both pages until this is fixed.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- A new `GetWorkTasksByProjectIdQuery` follows the same pattern as the existing
  `GetWorkTasksBySRSIdQuery`. It lives in
  `Application/WorkTasks/Queries/GetWorkTasksByProjectIdQuery.cs`.
- The handler calls `IWorkTaskRepository.GetByProjectIdAsync`.
- The API endpoint is added to `WorkTaskEndpoints.cs` following the existing pattern.
- Return type: `Result<IReadOnlyList<WorkTaskDto>>`.

---

## Implementation Scope — What must be done

- [ ] Create `Application/WorkTasks/Queries/GetWorkTasksByProjectIdQuery.cs`:
  ```csharp
  public record GetWorkTasksByProjectIdQuery(Guid ProjectId)
      : IRequest<Result<IReadOnlyList<WorkTaskDto>>>;

  internal sealed class GetWorkTasksByProjectIdQueryHandler(IWorkTaskRepository repo)
      : IRequestHandler<GetWorkTasksByProjectIdQuery, Result<IReadOnlyList<WorkTaskDto>>>
  {
      public async Task<Result<IReadOnlyList<WorkTaskDto>>> Handle(
          GetWorkTasksByProjectIdQuery request, CancellationToken ct)
      {
          var tasks = await repo.GetByProjectIdAsync(request.ProjectId, ct);
          var dtos = tasks.Select(t => new WorkTaskDto(...)).ToList();
          return Result<IReadOnlyList<WorkTaskDto>>.Success(dtos);
      }
  }
  ```
  Map all `WorkTaskDto` fields identically to the existing `GetWorkTasksBySRSIdQuery`.

- [ ] Add route to `WorkTaskEndpoints.cs`:
  ```csharp
  group.MapGet("/by-project/{projectId:guid}", async (Guid projectId, IMediator mediator) =>
  {
      var result = await mediator.Send(new GetWorkTasksByProjectIdQuery(projectId));
      return result.IsSuccess
          ? Results.Ok(result.Value)
          : Results.BadRequest(new { error = result.Error });
  });
  ```

- [ ] Run `dotnet build` — zero warnings, zero errors
- [ ] Run `dotnet test` — all tests pass

---

## Out of Scope — What must NOT be done

- Do not modify the existing `GetWorkTasksBySRSIdQuery`
- Do not add tests beyond the standard handler test pattern for the new query

---

## Test Expectations

- Unit tests required for:
  - `GetWorkTasksByProjectIdQueryHandler`: returns mapped DTOs for all tasks; empty list
    for project with no tasks
- Edge cases to cover: n/a

---

## Open Questions

- None.
