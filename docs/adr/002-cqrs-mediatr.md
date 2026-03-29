---
category: adr
id: "002"
title: "CQRS dispatch via MediatR"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [cqrs, clean-architecture]
---

# ADR-002: CQRS dispatch via MediatR

## Status
`accepted`

## Context
Agent actions (assign task, resolve gate, start sprint) are discrete write operations.
Read operations (board state, agent activity) need to stay fast and independent.
The API layer must remain thin. Pipeline behaviors (validation, logging) should be
composable without modifying individual handlers.

## Decision
Use CQRS: every operation is either a Command (write) or a Query (read), never mixed.
MediatR dispatches both from controllers. FluentValidation validators run as pipeline behaviors
before any command handler executes. Return types are DTOs or primitive IDs — never domain entities.

## Consequences

**Positive:**
- Read and write models evolve independently
- Pipeline behaviors (validation, logging) compose without touching handlers
- Future event sourcing requires no structural change — commands already map to intents
- Controllers are trivially thin: one `mediator.Send()` call per action

**Trade-offs:**
- More files per feature (command + handler + validator + DTO)
- MediatR is an external dependency in the Application layer (acceptable: it is a dispatch
  abstraction, not a business concern)

## Alternatives Considered

### Option A: Custom command bus (hand-rolled dispatcher)
Full control, no external dependency in Application.
Rejected because the implementation cost is non-trivial and MediatR's pipeline behavior
system (validation, logging, transactions) would need to be reproduced. The benefit does
not justify the effort for a solo project.

### Option B: Direct service calls from controllers
`IProjectService`, `ITaskService` etc. injected into controllers.
Rejected because it conflates read and write paths, makes pipeline behaviors harder to
attach uniformly, and encourages fat controllers over time.

## References
- See `cqrs.md` for command/query conventions and DTO rules
- See ADR-001 for the layer model that CQRS enforces
