---
category: adr
id: "001"
title: "Clean Architecture layer model"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [clean-architecture, solid]
---

# ADR-001: Clean Architecture layer model

## Status
`accepted`

## Context
ChaosForge has a complex domain (agents, revision gates, LLM providers) that must remain
testable in isolation. Infrastructure concerns (EF Core, LlamaSharp, SignalR) must be
swappable without touching business logic. The codebase is solo/hobby but AI-assisted,
so clear, enforceable boundaries reduce the risk of an AI agent placing logic in the wrong layer.

## Decision
Adopt Clean Architecture with four layers: Domain → Application → Infrastructure, API.
Dependencies point inward only. Domain has zero external NuGet dependencies.
Each layer registers itself via a DI extension method; only API calls all three.

## Consequences

**Positive:**
- Domain and Application are fully unit-testable without a database or HTTP stack
- LLM providers, persistence, and transport are independently swappable
- Layer violations are detectable by project reference analysis

**Trade-offs:**
- More projects and indirection than a simple layered monolith
- Every new feature requires mapping between layers (domain entity → DTO)

## Alternatives Considered

### Option A: Vertical Slice Architecture
Feature folders own their own handler, DB access, and DTO in one slice.
Rejected because cross-cutting agent orchestration spans multiple domain concerns simultaneously;
vertical slices would scatter the domain model across features and make the agent workflow harder to follow.

### Option B: Traditional N-tier (Controller → Service → Repository)
Simpler, familiar structure with no strict dependency rule.
Rejected because it provides no enforcement boundary — an AI agent or future contributor could
trivially couple a service to a concrete EF DbContext, eroding testability over time.

## References
- See `clean-architecture.md` for layer rules and DI registration pattern
- See `cqrs.md` (ADR-002) for the Command/Query pattern that reinforces Application layer boundaries
