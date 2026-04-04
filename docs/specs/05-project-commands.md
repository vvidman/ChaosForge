---
category: specs
title: "Project Commands"
branch: "project-cmds"
status: ready
date: "2026-04-03"
related_domain: [Project]
related_adr: []
---

# Feature Spec — Project Commands

<!-- Reference this file in the implementation agent with: implement @docs/specs/05-project-commands.md -->

---

## Context

The first Application layer use cases. A user creates a project and advances it through its
lifecycle phases. These are write operations (commands in CQRS) — no queries in this spec.
Depends on: Application Pipeline (spec 04) and Repository Interfaces (spec 02).

---

## Domain Impact

- New or modified entity: none
- New domain event: none (events are already defined in spec 03)
- New interface: none

---

## Architecture Decisions

- Each command is a `record` implementing `IRequest<Result>` (non-generic — no return value).
- Each command has a corresponding validator (`AbstractValidator<TCommand>`) in the same
  folder.
- Each handler is `internal sealed` and lives in the same file as its command.
- Handlers load the aggregate via repository, call domain methods, then call
  `IUnitOfWork.SaveChangesAsync`. They do NOT call `SaveChanges` directly on DbContext.
- If the entity is not found, the handler returns `Result.Failure("Project not found.")`.
- If a `DomainException` is thrown, it is caught in the handler and returned as
  `Result.Failure(ex.Message)`.
- Commands are in `Application/Projects/Commands/`.

---

## Implementation Scope — What must be done

- [ ] `CreateProjectCommand` + `CreateProjectCommandValidator` + `CreateProjectCommandHandler`
  ```
  Command properties: Name (string), Description (string), Deadline (DateTime?)
  Validator: Name not empty, Description not empty
  Handler: new Project(Name, Description, Deadline) → AddAsync → SaveChangesAsync
  ```
- [ ] `TransitionProjectCommand` + `TransitionProjectCommandValidator` + handler
  ```
  Command properties: ProjectId (Guid), NewStatus (ProjectStatus)
  Validator: ProjectId not empty
  Handler: GetByIdAsync → TransitionTo(NewStatus) → SaveChangesAsync
  ```
- [ ] `UpdateProjectDescriptionCommand` + validator + handler
  ```
  Command properties: ProjectId (Guid), Description (string)
  Validator: ProjectId not empty, Description not empty
  Handler: GetByIdAsync → UpdateDescription(Description) → SaveChangesAsync
  ```
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement project queries (separate spec)
- Do not implement any other entity's commands here
- Do not register DI in this spec — that is handled by `AddApplication()`
- Do not dispatch domain events — that is an Infrastructure concern (separate spec)

---

## Test Expectations

- Unit tests required for:
  - Each handler: happy path, entity-not-found path, domain exception path
  - Each validator: valid input passes, invalid input fails with correct error messages
- Edge cases to cover: `TransitionTo` called with current status (same step) must fail

---

## Open Questions

- None.
