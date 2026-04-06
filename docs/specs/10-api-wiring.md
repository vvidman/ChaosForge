---
category: specs
title: "API Wiring and Project Endpoints"
branch: "api-wiring"
status: done
date: "2026-04-03"
related_domain: [Project]
related_adr: []
---

# Feature Spec — API Wiring and Project Endpoints

<!-- Reference this file in the implementation agent with: implement @docs/specs/10-api-wiring.md -->

---

## Context

The Application and Infrastructure layers are fully wired; this spec connects them to the
HTTP layer. It sets up DI registration in `Program.cs`, configures the SQLite database,
and exposes the first working HTTP endpoints for `Project` commands. After this spec,
the first end-to-end request can be made with `curl` or Swagger.
Depends on: specs 04, 05, 08, 09.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Minimal API style (no controllers) — use `app.MapGroup("/api/projects")` and route
  handlers that send commands via `IMediator`.
- `Program.cs` calls `builder.Services.AddApplication()` and
  `builder.Services.AddInfrastructure(builder.Configuration)`.
- Connection string in `appsettings.Development.json`:
  `"ConnectionStrings": { "DefaultConnection": "Data Source=chaosforge.db" }`.
- `app.MapOpenApi()` is sufficient for dev-time exploration (available in .NET 9+/10).
  Do not add Swashbuckle.
- Endpoint responses: return `Results.Ok()` on success, `Results.BadRequest(error)` on
  `Result.IsSuccess == false`, `Results.ValidationProblem(...)` when
  `ValidationException` is caught.
- Global exception handling: add a `app.UseExceptionHandler` middleware that catches
  unhandled exceptions and returns `500` with a generic message — no stack traces in
  production responses.
- Database auto-migration on startup: call
  `await context.Database.MigrateAsync()` in the startup pipeline (acceptable for a
  hobby project; note this is not recommended for production).
- Route group: `/api/projects`
- Do not add authentication or authorization in this spec.

---

## Implementation Scope — What must be done

- [x] Update `Program.cs`:
  - Call `AddApplication()` and `AddInfrastructure(configuration)`
  - Add `app.UseExceptionHandler` middleware
  - Call `MigrateAsync()` on startup
  - Map `OpenApi` in Development environment
- [x] Create `ProjectEndpoints.cs` in `API/Endpoints/` with:
  - `POST /api/projects` → `CreateProjectCommand`
  - `POST /api/projects/{id}/transition` → `TransitionProjectCommand`
  - `PATCH /api/projects/{id}/description` → `UpdateProjectDescriptionCommand`
- [x] Create request DTOs for each endpoint in `API/Endpoints/` (do NOT pass MediatR
  commands directly as request bodies — map from DTO to command in the handler lambda):
  ```
  CreateProjectRequest  { Name, Description, Deadline? }
  TransitionProjectRequest  { NewStatus }
  UpdateProjectDescriptionRequest  { Description }
  ```
- [x] Map FluentValidation `ValidationException` to `Results.ValidationProblem` in the
  exception handler
- [x] Verify end-to-end with `dotnet run` and at least one manual `curl` call to
  `POST /api/projects` — include the successful response in the implementation notes
  - `curl -X POST http://localhost:5143/api/projects -H "Content-Type: application/json" -d '{"name":"Test Project","description":"A smoke-test project","deadline":null}'` → HTTP 200

---

## Out of Scope — What must NOT be done

- Do not add endpoints for any entity other than `Project` in this spec
- Do not add authentication
- Do not add Swashbuckle or NSwag
- Do not add CORS configuration yet

---

## Test Expectations

- Unit tests required for: none in this spec
- Edge cases to cover: n/a (manual smoke test via curl is sufficient)

---

## Open Questions

- None.
