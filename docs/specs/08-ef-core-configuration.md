---
category: specs
title: "EF Core Configuration"
branch: "ef-core-cfg"
status: ready
date: "2026-04-03"
related_domain: [Project, UseCase, URS, SRS, WorkTask, TaskAttempt, RevisionGate, AgentSlot, AgentInstance]
related_adr: []
---

# Feature Spec — EF Core Configuration

<!-- Reference this file in the implementation agent with: implement @docs/specs/08-ef-core-configuration.md -->

---

## Context

The Infrastructure layer needs a `DbContext` and Fluent API entity configurations before
repositories can be implemented. This spec produces a working SQLite-backed `AppDbContext`
and all `IEntityTypeConfiguration<T>` classes. No data is seeded; no migrations are run
in this spec — only the configuration code.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `AppDbContext` lives in `Infrastructure/Persistence/`.
- One `IEntityTypeConfiguration<T>` class per entity, in `Infrastructure/Persistence/Configurations/`.
- All configuration is Fluent API only — no data annotations on domain entities.
- Primary keys are `Guid`, mapped to `TEXT` in SQLite (EF Core default for Guid on SQLite).
- All `string` required properties are configured with `IsRequired()` and a `HasMaxLength`.
  Suggested limits: Name/Title = 200, Description/Output/AgentOutput = 4000,
  PersonaName = 100, RejectionReason/EditNote = 1000. These can be revised later.
- Nullable `string?` properties: `IsRequired(false)`, same max lengths.
- Enum properties: stored as `int` (EF Core default — do not override to string).
- `EntityBase<TId>` properties (`Id`, `CreatedAt`) are configured once via a shared method
  or base configuration — do not repeat in every configuration class.
- `AppDbContext` constructor accepts `DbContextOptions<AppDbContext>` and calls `base(options)`.
- `AppDbContext` overrides `OnModelCreating` and calls `modelBuilder.ApplyConfigurationsFromAssembly`.
- `AppDbContext` implements `IUnitOfWork` by delegating `SaveChangesAsync` to
  `base.SaveChangesAsync`.

---

## Implementation Scope — What must be done

- [ ] Create `AppDbContext : DbContext, IUnitOfWork` in `Infrastructure/Persistence/`
  - `DbSet<Project>`, `DbSet<UseCase>`, `DbSet<URS>`, `DbSet<SRS>`, `DbSet<WorkTask>`,
    `DbSet<TaskAttempt>`, `DbSet<RevisionGate>`, `DbSet<AgentSlot>`, `DbSet<AgentInstance>`
  - `OnModelCreating` applies all configurations from the assembly
  - Implements `IUnitOfWork.SaveChangesAsync`
- [ ] Create `ProjectConfiguration : IEntityTypeConfiguration<Project>`
  - Table name: `Projects`
  - All string fields with `IsRequired` + `HasMaxLength`
  - `Deadline` is optional
- [ ] `UseCaseConfiguration` — table `UseCases`, FK to `Projects`
- [ ] `URSConfiguration` — table `URSs`, FK to `UseCases`
- [ ] `SRSConfiguration` — table `SRSs`, FK to `URSs`
- [ ] `WorkTaskConfiguration` — table `WorkTasks`, FK to `SRSs`, nullable FK `SprintId` (no nav)
- [ ] `TaskAttemptConfiguration` — table `TaskAttempts`, FK to `WorkTasks`
- [ ] `RevisionGateConfiguration` — table `RevisionGates`, FK to `Projects`
- [ ] `AgentSlotConfiguration` — table `AgentSlots`, FK to `Projects`
- [ ] `AgentInstanceConfiguration` — table `AgentInstances`, FK to `Projects`,
  nullable `CurrentTaskId` (no nav)
- [ ] Add initial EF Core migration: `dotnet ef migrations add InitialCreate`
  (run from the `ChaosForge.API` project, targeting Infrastructure)
- [ ] Verify migration generates without error: `dotnet ef database update`
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not seed data
- Do not implement repositories yet (separate spec)
- Do not add DI registration yet (separate spec)
- Do not add navigation properties to entities — keep entities clean from EF concerns;
  shadow properties for FKs are acceptable

---

## Test Expectations

- Unit tests required for: none in this spec
- Edge cases to cover: n/a — migration generation is the verification mechanism here

---

## Open Questions

- None.
