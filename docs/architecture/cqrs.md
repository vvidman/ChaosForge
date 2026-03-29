---
category: architecture
principle: "CQRS with MediatR"
last_updated: "2026-03-29"
related_adr: [ADR-002]
---

# CQRS with MediatR

## What it is
Every operation is either a Command (write — returns void or a new ID) or a Query (read —
never mutates state). MediatR dispatches both from the API layer to Application handlers.

## Why we apply it
Agent actions map cleanly to Commands. Query side stays independent and fast. The structure
enables future event sourcing without structural changes.

## How we apply it

### Commands
`CreateProjectCommand`, `StartSprintCommand`, `ResolveRevisionGateCommand`, `AssignTaskCommand`
— each validated by a FluentValidation validator before the handler runs.

### Queries
`GetBacklogQuery`, `GetSprintBoardQuery`, `GetAgentActivityQuery`, `GetProjectQuery`
— read-only, no side effects.

### DTOs
Separate request/response records per use case. Domain entities never escape the Application
boundary as return values — always mapped to a response DTO.

## Rules
- **Must**: commands mutate state, queries do not — never mixed in one handler
- **Must**: every command has a corresponding FluentValidation validator
- **Must**: return types are DTOs or primitive IDs — never domain entities
- **?** Does this handler both write to the database and return a projected read model? If yes → split it.
- **?** Is this query triggering a domain event? If yes → it is a command, not a query.

## Anti-patterns

### Mixed handler
A handler that persists data *and* returns a rich read model. Commands return only the new
entity ID; callers issue a separate query if they need the full object.

### Business logic in controllers
`mediator.Send()` is the only non-trivial call a controller makes. Validation, authorization,
and orchestration belong in handlers or domain services.

## References
- See `clean-architecture.md` — layer rules that CQRS enforces
- See `agent-design.md` — how agent workers issue Commands
