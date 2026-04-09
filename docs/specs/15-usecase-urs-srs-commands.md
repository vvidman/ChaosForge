---
category: specs
title: "UseCase, URS and SRS Commands"
branch: "req-commands"
status: done
date: "2026-04-07"
related_domain: [UseCase, URS, SRS]
related_adr: []
---

# Feature Spec — UseCase, URS and SRS Commands

<!-- Reference this file in the implementation agent with: implement @docs/specs/15-usecase-urs-srs-commands.md -->

---

## Context

UseCase, URS, and SRS represent the requirements pipeline. Without write operations, the
agent orchestrator cannot persist the output of the Business Analyst (UseCases), the URS
author, or the SRS author. This spec covers creation and the human-edit mutations for URS
and SRS. Follows the same command handler pattern established in specs 05 and 06.
Depends on: specs 04 (pipeline), 02 (repository interfaces).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same conventions as specs 05 and 06: `record` commands, `AbstractValidator`, `internal sealed`
  handler, all in the same file, in `Application/UseCases/Commands/`,
  `Application/URS/Commands/`, `Application/SRS/Commands/`.
- Return type for all commands: `Result` (non-generic).
- All handlers wrap domain calls in `try/catch (DomainException ex)` → `Result.Failure`.
- `CreateURSCommand` and `CreateSRSCommand` do not validate that parent ids exist in the
  database — referential integrity is enforced by the database.
- `ApplyHumanEditToURSCommand` and `ApplyHumanEditToSRSCommand` model the human revision gate
  edit action at the entity level.

---

## Implementation Scope — What must be done

### UseCase Commands

- [x] `CreateUseCaseCommand(Guid ProjectId, string Title, string Description, int Priority)`
  + validator (ProjectId not empty, Title not empty, Description not empty, Priority >= 0)
  + handler: `new UseCase(...)` → `AddAsync` → `SaveChangesAsync`

- [x] `UpdateUseCasePriorityCommand(Guid UseCaseId, int Priority)`
  + validator (UseCaseId not empty, Priority >= 0)
  + handler: `GetByIdAsync` → not-found check → `UpdatePriority(Priority)` → `SaveChangesAsync`

### URS Commands

- [x] `CreateURSCommand(Guid UseCaseId, string Title, string Description)`
  + validator (UseCaseId not empty, Title not empty, Description not empty)
  + handler: `new URS(...)` → `AddAsync` → `SaveChangesAsync`

- [x] `ApplyHumanEditToURSCommand(Guid URSId, string EditedDescription, string Note)`
  + validator (URSId not empty, EditedDescription not empty, Note not empty)
  + handler: `GetByIdAsync` → not-found check → `ApplyHumanEdit(...)` → `SaveChangesAsync`

### SRS Commands

- [x] `CreateSRSCommand(Guid URSId, string Title, string TechnicalDescription)`
  + validator (URSId not empty, Title not empty, TechnicalDescription not empty)
  + handler: `new SRS(...)` → `AddAsync` → `SaveChangesAsync`

- [x] `ApplyHumanEditToSRSCommand(Guid SRSId, string EditedDescription, string Note)`
  + validator (SRSId not empty, EditedDescription not empty, Note not empty)
  + handler: `GetByIdAsync` → not-found check → `ApplyHumanEdit(...)` → `SaveChangesAsync`

- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add API endpoints in this spec — that is spec 18
- Do not add delete operations — not in scope for this project
- Do not dispatch domain events — that is spec 19

---

## Test Expectations

- Unit tests required for:
  - Each create handler: happy path, domain exception path
  - Each update/edit handler: happy path, not-found path, domain exception path
  - Each validator: required fields fail on empty/null
  - `UpdateUseCasePriorityCommand` validator: Priority < 0 fails
- Edge cases to cover: `ApplyHumanEditToURSCommand` with whitespace-only EditedDescription
  triggers DomainException (caught by handler → Failure)

---

## Open Questions

- None.
