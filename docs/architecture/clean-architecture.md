---
category: architecture
principle: "Clean Architecture"
last_updated: "2026-03-29"
related_adr: [ADR-001]
---

# Clean Architecture

## What it is
Layers arranged so dependencies point inward only: Domain ← Application ← Infrastructure,
with API sitting atop Application. Inner layers define interfaces; outer layers implement them.

## Why we apply it
Keeps business logic independent of infrastructure concerns (database, HTTP, LLM providers).
Domain and Application are fully testable in isolation; providers and persistence are swappable
without touching the core.

## How we apply it

### Layer boundaries

| Layer | Project | Allowed dependencies |
|---|---|---|
| Domain | ChaosForge.Domain | None — zero NuGet |
| Application | ChaosForge.Application | Domain only |
| Infrastructure | ChaosForge.Infrastructure | Application, Domain |
| API | ChaosForge.API | Application (via MediatR) |

### Dependency inversion at boundaries
Interfaces (`ILLMProvider`, `IProjectRepository`, `IUnitOfWork`) are declared in Domain or
Application. Infrastructure implements them. API wires everything via DI extension methods:
`AddDomainServices()` / `AddApplicationServices()` / `AddInfrastructureServices(configuration)`.

### State change rule
All state changes go through domain entities and domain events — no direct DB writes that
bypass the domain model.

## Rules
- **Must**: `ChaosForge.Domain.csproj` must have zero `<PackageReference>` nodes
- **Must**: no `using ChaosForge.Infrastructure` anywhere in Application
- **Must**: controllers call only `mediator.Send()` and SignalR notify — no business logic
- **Must**: no `static` classes for business logic
- **?** Can this class be unit-tested without a database or HTTP call? If no → it belongs in Infrastructure.
- **?** Does this interface live in the layer that *uses* it, not the layer that *implements* it?

## Anti-patterns

### Infrastructure bleed into Application
Referencing `DbContext`, `HttpClient`, or any LLM SDK directly from an Application handler.
Declare an interface in Domain/Application and inject it.

### Fat controller
Placing orchestration, validation, or domain logic in a controller.
Everything goes through a Command or Query handler.

## References
- See `cqrs.md` — the Command/Query split that reinforces Application layer boundaries
- See `llm-strategy.md` — how `ILLMProvider` is resolved at runtime
