---
category: adr
id: "008"
title: "SQLite with EF Core for persistence"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [clean-architecture]
---

# ADR-008: SQLite with EF Core for persistence

## Status
`accepted`

## Context
ChaosForge stores projects, tasks, attempts, and gate decisions. The data model is
relational with clear aggregate boundaries. The project must run locally without
external infrastructure. LINQ-based queries are preferred over raw SQL for type safety
and refactoring support.

## Decision
Use SQLite as the database and EF Core as the ORM. All DB access goes through EF Core —
no raw SQL. Migrations are managed via `dotnet ef migrations`. The `DbContext` and all
repository implementations live exclusively in Infrastructure.

## Consequences

**Positive:**
- Zero external infrastructure: the database is a single file
- EF Core migrations provide a versioned schema history
- LINQ queries are type-safe and refactor-friendly
- Switching to Postgres later requires only a provider swap in `AddInfrastructureServices`

**Trade-offs:**
- SQLite has limited concurrency under write-heavy load (acceptable: agent workers
  write sequentially per task)
- EF Core adds abstraction overhead vs. Dapper for complex read queries

## Alternatives Considered

### Option A: PostgreSQL with EF Core
Production-grade relational DB with full concurrency support.
Rejected because it requires Docker or a local Postgres installation. The zero-dependency
local setup constraint takes precedence. If ChaosForge ever needs concurrent multi-user
access, this ADR should be revisited.

### Option B: Dapper with raw SQL
Lightweight, fast, explicit. No ORM magic.
Rejected because the relational model has enough complexity (aggregate roots, navigation
properties, migrations) that the boilerplate cost of raw SQL outweighs the performance gain.
Type-safe LINQ queries reduce the risk of an AI agent writing structurally broken queries.

## References
- See `clean-architecture.md` — DbContext is Infrastructure-only
- See ADR-001 for the layer model that keeps persistence concerns out of Domain and Application
