---
category: specs
title: "UseCase, URS and SRS Queries"
branch: "req-queries"
status: ready
date: "2026-04-07"
related_domain: [UseCase, URS, SRS]
related_adr: []
---

# Feature Spec — UseCase, URS and SRS Queries

<!-- Reference this file in the implementation agent with: implement @docs/specs/13-usecase-urs-srs-queries.md -->

---

## Context

The requirements pipeline entities — UseCase, URS, SRS — form a strict hierarchy:
Project → UseCase → URS → SRS. Without query handlers for these, the frontend cannot
display requirement details or navigate the hierarchy. All three are grouped here because
their query patterns are structurally identical (get by id, get by parent id).
Depends on spec 11 (query pattern established).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Same conventions as specs 11 and 12.
- Handlers live in `Application/UseCases/Queries/`, `Application/URS/Queries/`,
  `Application/SRS/Queries/`.
- `URSDto` includes `HumanEditNote` (nullable) — it may be null before any human edit.
- `SRSDto` includes `HumanEditNote` (nullable).
- No cross-entity joins — queries return only the fields of the requested entity.

---

## Implementation Scope — What must be done

### UseCase

- [ ] Create `UseCaseDto` record:
  ```
  Guid Id, Guid ProjectId, string Title, string Description, int Priority, DateTime CreatedAt
  ```
- [ ] `GetUseCaseByIdQuery(Guid UseCaseId)` → `Result<UseCaseDto>`
- [ ] `GetUseCasesByProjectIdQuery(Guid ProjectId)` → `Result<IReadOnlyList<UseCaseDto>>`
  - Handler: `IUseCaseRepository.GetByProjectIdAsync`

### URS

- [ ] Create `URSDto` record:
  ```
  Guid Id, Guid UseCaseId, string Title, string Description, string? HumanEditNote, DateTime CreatedAt
  ```
- [ ] `GetURSByIdQuery(Guid URSId)` → `Result<URSDto>`
- [ ] `GetURSsByUseCaseIdQuery(Guid UseCaseId)` → `Result<IReadOnlyList<URSDto>>`
  - Handler: `IURSRepository.GetByUseCaseIdAsync`

### SRS

- [ ] Create `SRSDto` record:
  ```
  Guid Id, Guid URSId, string Title, string TechnicalDescription, string? HumanEditNote, DateTime CreatedAt
  ```
- [ ] `GetSRSByIdQuery(Guid SRSId)` → `Result<SRSDto>`
- [ ] `GetSRSsByURSIdQuery(Guid URSId)` → `Result<IReadOnlyList<SRSDto>>`
  - Handler: `ISRSRepository.GetByURSIdAsync`

- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add API endpoints in this spec — that is spec 18
- Do not return child entities from parent queries (no UseCase → URS nesting)
- Do not add commands in this spec — that is spec 15

---

## Test Expectations

- Unit tests required for:
  - Each `GetById` handler: not-found returns Failure, found maps all fields correctly
  - Each `GetByParentId` handler: empty result returns empty list (not Failure)
- Edge cases to cover: `HumanEditNote` is null in DTO when entity field is null

---

## Open Questions

- None.
