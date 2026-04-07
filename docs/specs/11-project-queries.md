---
category: specs
title: "Project Queries"
branch: "project-qry"
status: done
date: "2026-04-07"
related_domain: [Project]
related_adr: []
---

# Feature Spec — Project Queries

<!-- Reference this file in the implementation agent with: implement @docs/specs/11-project-queries.md -->

---

## Context

The project commands are in place but there is no way to read project data. This spec
introduces the query side of CQRS for the `Project` aggregate: listing all projects and
fetching a single project by id. These queries are the first read operations in the
Application layer and establish the query handler pattern for all subsequent query specs.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Queries are `record` types implementing `IRequest<TResponse>` where `TResponse` is a
  DTO — not the entity itself. Entities must never leave the Application layer.
- Query result DTOs are `record` types defined in the same file as the query, in a nested
  or adjacent position. Suffix: `Dto` (e.g. `ProjectDto`, `ProjectSummaryDto`).
- `ProjectSummaryDto` is used for list responses (lighter); `ProjectDto` for single fetch
  (all fields).
- Query handlers are `internal sealed` and live in `Application/Projects/Queries/`.
- Query handlers inject repository interfaces only — no `IUnitOfWork` needed.
- If a single-entity query finds nothing, the handler returns `Result<ProjectDto>.Failure("Project not found.")`.
- List queries never fail — they return an empty list if there are no results.
- No pagination in this spec — `GetAllProjects` returns the full list. Pagination is a
  future concern.
- Return type for list query: `Result<IReadOnlyList<ProjectSummaryDto>>`.
- Return type for single query: `Result<ProjectDto>`.

---

## Implementation Scope — What must be done

- [x] Create `ProjectSummaryDto` record:
  ```
  Guid Id, string Name, string Description, ProjectStatus Status, DateTime? Deadline, DateTime CreatedAt
  ```
- [x] Create `ProjectDto` record (same fields as summary — no extra fields at this stage):
  ```
  Guid Id, string Name, string Description, ProjectStatus Status, DateTime? Deadline, DateTime CreatedAt
  ```
- [x] Create `GetAllProjectsQuery` record implementing `IRequest<Result<IReadOnlyList<ProjectSummaryDto>>>`
  - No properties
- [x] Create `GetAllProjectsQueryHandler` (`internal sealed`)
  - Calls `IProjectRepository.GetAllAsync()`
  - Maps each `Project` to `ProjectSummaryDto`
  - Returns `Result<IReadOnlyList<ProjectSummaryDto>>.Success(...)`
- [x] Create `GetProjectByIdQuery` record implementing `IRequest<Result<ProjectDto>>`
  - Property: `Guid ProjectId`
- [x] Create `GetProjectByIdQueryHandler` (`internal sealed`)
  - Calls `IProjectRepository.GetByIdAsync(ProjectId)`
  - Returns `Result<ProjectDto>.Failure("Project not found.")` if null
  - Maps `Project` to `ProjectDto` and returns `Result<ProjectDto>.Success(...)`
- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not expose navigation data (UseCases, RevisionGates) from project queries
- Do not add pagination
- Do not add filtering or sorting
- Do not create API endpoints in this spec — that is spec 18

---

## Test Expectations

- Unit tests required for:
  - `GetAllProjectsQueryHandler`: returns empty list when repository returns empty, maps all
    fields correctly for non-empty list
  - `GetProjectByIdQueryHandler`: returns Failure when not found, maps all fields correctly
    when found
- Edge cases to cover: `GetAllProjects` with empty store returns `IsSuccess = true` and
  empty list (not Failure)

---

## Open Questions

- None.
