---
category: specs
title: "Repository Interfaces"
branch: "repo-ifaces"
status: ready
date: "2026-04-03"
related_domain: [Project, UseCase, URS, SRS, WorkTask, TaskAttempt, RevisionGate, AgentSlot, AgentInstance]
related_adr: []
---

# Feature Spec — Repository Interfaces

<!-- Reference this file in the implementation agent with: implement @docs/specs/02-repository-interfaces.md -->

---

## Context

The domain entities exist but there is no persistence contract yet. Repository interfaces
belong in the Domain layer — they define what the Application layer needs from persistence
without coupling to EF Core or any other technology. Infrastructure will implement them;
this spec only defines the contracts.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: `IRepository<TEntity, TId>`, `IProjectRepository`, `IUseCaseRepository`,
  `IURSRepository`, `ISRSRepository`, `IWorkTaskRepository`, `ITaskAttemptRepository`,
  `IRevisionGateRepository`, `IAgentSlotRepository`, `IAgentInstanceRepository`,
  `IUnitOfWork`

---

## Architecture Decisions

- Interfaces live in `ChaosForge.Domain/Repositories/`.
- A generic `IRepository<TEntity, TId>` base interface defines the common contract;
  entity-specific interfaces extend it with query methods relevant to that entity only.
- `IUnitOfWork` is a separate interface with a single `SaveChangesAsync` method.
  It does NOT extend `IRepository`.
- All methods are `async` and accept `CancellationToken cancellationToken = default`.
- Return types: single entity lookups return `Task<TEntity?>` (nullable — not found is not
  an exception at the repository level). Collection queries return `Task<IReadOnlyList<TEntity>>`.
- No `IQueryable` exposure — repositories return materialized results only.
- No `Update` method on any repository — EF Core change tracking handles updates;
  the Application layer loads, mutates via domain methods, then calls `SaveChangesAsync`.
- `Delete` is included on the generic interface as a soft contract but entity-specific
  interfaces may omit it if deletion is not a valid operation for that aggregate.
- XML doc comment on every interface member.

---

## Implementation Scope — What must be done

- [ ] Create `IRepository<TEntity, TId>` in `Domain/Repositories/`:
  ```
  Task<TEntity?> GetByIdAsync(TId id, CancellationToken cancellationToken = default)
  Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
  void Delete(TEntity entity)
  ```
- [ ] `IProjectRepository : IRepository<Project, Guid>`
  ```
  Task<IReadOnlyList<Project>> GetAllAsync(CancellationToken cancellationToken = default)
  ```
- [ ] `IUseCaseRepository : IRepository<UseCase, Guid>`
  ```
  Task<IReadOnlyList<UseCase>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
  ```
- [ ] `IURSRepository : IRepository<URS, Guid>`
  ```
  Task<IReadOnlyList<URS>> GetByUseCaseIdAsync(Guid useCaseId, CancellationToken cancellationToken = default)
  ```
- [ ] `ISRSRepository : IRepository<SRS, Guid>`
  ```
  Task<IReadOnlyList<SRS>> GetByURSIdAsync(Guid ursId, CancellationToken cancellationToken = default)
  ```
- [ ] `IWorkTaskRepository : IRepository<WorkTask, Guid>`
  ```
  Task<IReadOnlyList<WorkTask>> GetBySRSIdAsync(Guid srsId, CancellationToken cancellationToken = default)
  Task<IReadOnlyList<WorkTask>> GetBySprintIdAsync(Guid sprintId, CancellationToken cancellationToken = default)
  Task<IReadOnlyList<WorkTask>> GetByStatusAsync(WorkTaskStatus status, CancellationToken cancellationToken = default)
  ```
- [ ] `ITaskAttemptRepository : IRepository<TaskAttempt, Guid>`
  ```
  Task<IReadOnlyList<TaskAttempt>> GetByWorkTaskIdAsync(Guid workTaskId, CancellationToken cancellationToken = default)
  ```
- [ ] `IRevisionGateRepository : IRepository<RevisionGate, Guid>`
  ```
  Task<IReadOnlyList<RevisionGate>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
  Task<RevisionGate?> GetOpenByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
  ```
- [ ] `IAgentSlotRepository : IRepository<AgentSlot, Guid>`
  ```
  Task<IReadOnlyList<AgentSlot>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
  ```
- [ ] `IAgentInstanceRepository : IRepository<AgentInstance, Guid>`
  ```
  Task<IReadOnlyList<AgentInstance>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
  Task<IReadOnlyList<AgentInstance>> GetByStatusAsync(AgentInstanceStatus status, CancellationToken cancellationToken = default)
  ```
- [ ] `IUnitOfWork` in `Domain/Repositories/`:
  ```
  Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  ```
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement any repository — that belongs to the Infrastructure spec
- Do not register anything in DI — that belongs to the Infrastructure spec
- Do not add EF Core or any other package to the Domain project
- Do not modify entity classes

---

## Test Expectations

- Unit tests required for: none — interfaces have no logic to test
- Edge cases to cover: n/a

---

## Open Questions

- None.
