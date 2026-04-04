---
category: specs
title: "RevisionGate Commands"
branch: "gate-cmds"
status: ready
date: "2026-04-03"
related_domain: [RevisionGate, Project]
related_adr: []
---

# Feature Spec — RevisionGate Commands

<!-- Reference this file in the implementation agent with: implement @docs/specs/07-revision-gate-commands.md -->

---

## Context

Revision gates are the human checkpoints between major project phases. This spec covers
creating a gate (by the orchestrator) and the three human resolution actions: Accept,
EditAndAccept, and Reject. These are the commands the API will expose to the frontend
for the human judge role.
Depends on: Application Pipeline (spec 04), Repository Interfaces (spec 02).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Commands live in `Application/RevisionGates/Commands/`.
- Same structural conventions as specs 05 and 06.
- `OpenRevisionGateCommand` is the "create" command — named to reflect the domain concept,
  not CRUD.
- Only one gate per project may be `Open` at a time. The handler must check
  `IRevisionGateRepository.GetOpenByProjectIdAsync` before creating a new gate; if one is
  already open, return `Result.Failure("A revision gate is already open for this project.")`.
- Return type for all commands: `Result` (non-generic).

---

## Implementation Scope — What must be done

- [ ] `OpenRevisionGateCommand` + validator + handler
  ```
  Command: ProjectId (Guid), Type (RevisionGateType), AgentOutput (string)
  Validator: ProjectId not empty, AgentOutput not empty
  Handler: GetOpenByProjectIdAsync → fail if already open
           new RevisionGate(...) → AddAsync → SaveChangesAsync
  ```
- [ ] `AcceptRevisionGateCommand` + validator + handler
  ```
  Command: RevisionGateId (Guid)
  Validator: not empty
  Handler: GetByIdAsync → Accept() → SaveChangesAsync
  ```
- [ ] `EditAndAcceptRevisionGateCommand` + validator + handler
  ```
  Command: RevisionGateId (Guid), EditedOutput (string)
  Validator: not empty, EditedOutput not empty
  Handler: GetByIdAsync → EditAndAccept(EditedOutput) → SaveChangesAsync
  ```
- [ ] `RejectRevisionGateCommand` + validator + handler
  ```
  Command: RevisionGateId (Guid), Reason (string)
  Validator: not empty, Reason not empty
  Handler: GetByIdAsync → Reject(Reason) → SaveChangesAsync
  ```
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement RevisionGate queries (separate spec)
- Do not trigger project status transition from the gate resolution handler —
  that is an orchestration concern belonging to a later spec
- Do not dispatch domain events

---

## Test Expectations

- Unit tests required for:
  - `OpenRevisionGateCommand` handler: already-open gate returns Failure
  - All resolution handlers: happy path, not-found, already-resolved (domain exception)
- Edge cases to cover: double-resolution attempt via handler

---

## Open Questions

- None.
