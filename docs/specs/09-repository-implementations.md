---
category: specs
title: "Repository Implementations"
branch: "repo-impl"
status: done
date: "2026-04-03"
related_domain: [Project, UseCase, URS, SRS, WorkTask, TaskAttempt, RevisionGate, AgentSlot, AgentInstance]
related_adr: []
---

# Feature Spec — Repository Implementations

<!-- Reference this file in the implementation agent with: implement @docs/specs/09-repository-implementations.md -->

---

## Context

The repository interfaces (spec 02) and EF Core configuration (spec 08) are in place.
This spec implements the concrete repository classes in Infrastructure and wires everything
into DI. After this spec, the Application layer command handlers can run end-to-end.
Depends on: specs 02 and 08.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- All repository implementations live in `Infrastructure/Persistence/Repositories/`.
- Each repository is `internal sealed` — not exposed outside Infrastructure.
- Each repository constructor injects `AppDbContext` directly (not `IUnitOfWork`) — the
  context is the source of truth for change tracking.
- No `SaveChangesAsync` call inside any repository method — that is the caller's
  (the Application handler's) responsibility via `IUnitOfWork`.
- `Delete(TEntity entity)` calls `_context.Set<TEntity>().Remove(entity)`.
- `AddAsync` calls `await _context.Set<TEntity>().AddAsync(entity, cancellationToken)`.
- Query methods use `AsNoTracking()` only for read-only queries that return lists;
  `GetByIdAsync` must NOT use `AsNoTracking()` because the Application layer mutates
  the returned entity.
- DI registration: a single `AddInfrastructure(this IServiceCollection services, IConfiguration configuration)`
  extension method in `Infrastructure/DependencyInjection.cs` that registers:
  - `AppDbContext` with SQLite connection string from configuration key `"ConnectionStrings:DefaultConnection"`
  - All repository implementations against their interfaces (Scoped lifetime)
  - `AppDbContext` as `IUnitOfWork` (Scoped lifetime, same instance)

---

## Implementation Scope — What must be done

- [x] Create `RepositoryBase<TEntity, TId>` in `Infrastructure/Persistence/Repositories/`
  implementing `IRepository<TEntity, TId>`:
  ```
  GetByIdAsync   → _context.Set<T>().FindAsync(id, ct)
  AddAsync       → _context.Set<T>().AddAsync(entity, ct)
  Delete         → _context.Set<T>().Remove(entity)
  ```
- [x] `ProjectRepository : RepositoryBase<Project, Guid>, IProjectRepository`
  - `GetAllAsync` → `_context.Projects.AsNoTracking().ToListAsync(ct)`
- [x] `UseCaseRepository : RepositoryBase<UseCase, Guid>, IUseCaseRepository`
- [x] `URSRepository : RepositoryBase<URS, Guid>, IURSRepository`
- [x] `SRSRepository : RepositoryBase<SRS, Guid>, ISRSRepository`
- [x] `WorkTaskRepository : RepositoryBase<WorkTask, Guid>, IWorkTaskRepository`
  - `GetByStatusAsync` — filter by `Status`
  - `GetBySprintIdAsync` — filter by `SprintId`
  - `GetBySRSIdAsync` — filter by `SRSId`
- [x] `TaskAttemptRepository : RepositoryBase<TaskAttempt, Guid>, ITaskAttemptRepository`
- [x] `RevisionGateRepository : RepositoryBase<RevisionGate, Guid>, IRevisionGateRepository`
  - `GetOpenByProjectIdAsync` — filter by `ProjectId` and `Status == Open`, `FirstOrDefaultAsync`
- [x] `AgentSlotRepository : RepositoryBase<AgentSlot, Guid>, IAgentSlotRepository`
- [x] `AgentInstanceRepository : RepositoryBase<AgentInstance, Guid>, IAgentInstanceRepository`
  - `GetByStatusAsync` — filter by `Status`
- [x] Create `DependencyInjection.cs` in `Infrastructure/` with `AddInfrastructure`
- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not call `AddInfrastructure` from the API yet — that is the API wiring spec
- Do not add caching
- Do not implement `IDomainEventDispatcher` yet

---

## Test Expectations

- Unit tests required for: none in this pass (integration tests with in-memory SQLite
  are a future spec)
- Edge cases to cover: n/a

---

## Open Questions

- None.
