---
category: toolchain
tool: "EF Core Migrations"
last_updated: "2026-03-29"
---

# EF Core Migrations

## Purpose
EF Core migrations version the database schema alongside the code. Every schema change —
new entity, new field, renamed column — must have a migration. Direct database edits are
forbidden. See ADR-008 for the SQLite + EF Core rationale.

## Prerequisites
- .NET 10 SDK
- EF Core CLI tools:

```bash
dotnet tool install --global dotnet-ef
# or update if already installed
dotnet tool update --global dotnet-ef
```

Verify:
```bash
dotnet ef --version
```

## Common Tasks

### Add a migration after a model change

```bash
dotnet ef migrations add <MigrationName> \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API
```

**Naming convention:** PascalCase, describes the schema change precisely.

| Good | Bad |
|---|---|
| `AddRevisionGateEntity` | `Migration1` |
| `AddTaskAttemptReviewNote` | `Update` |
| `MakeDeadlineNullable` | `Fix` |

### Apply migrations to local database

```bash
dotnet ef database update \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API
```

The SQLite database file is created automatically if it does not exist.

### List applied migrations

```bash
dotnet ef migrations list \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API
```

Applied migrations are marked with `[applied]`.

### Revert the last migration (local only)

```bash
# Roll back to the previous migration state
dotnet ef database update <PreviousMigrationName> \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API

# Then remove the migration file
dotnet ef migrations remove \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API
```

Only revert migrations that have not been committed to `dev`. Once a migration is in the
shared branch, add a corrective migration instead of removing.

### Generate SQL script (for review)

```bash
dotnet ef migrations script \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API \
  --output migration.sql
```

Useful for reviewing what a migration will actually execute before applying it.

## Workflow — migration in the implementation flow

1. Modify or add a domain entity in `ChaosForge.Domain`
2. Update `ChaosForgeDbContext` in `ChaosForge.Infrastructure` (entity configuration)
3. Add migration with a descriptive name
4. Apply migration locally — verify with `dotnet build` + `dotnet test`
5. Commit the migration file alongside the code change — never in a separate commit

**The migration file is part of the feature commit.** Reviewers check that the migration
matches the entity change and the name is descriptive.

## Troubleshooting

### `No DbContext was found`
**Cause:** The `--startup-project` flag is missing or points to the wrong project.
**Fix:** Always specify both `--project src/ChaosForge.Infrastructure` and `--startup-project src/ChaosForge.API`.

### `Unable to create an object of type 'ChaosForgeDbContext'`
**Cause:** The `IDesignTimeDbContextFactory` is missing or the connection string is not resolvable at design time.
**Fix:** Ensure `ChaosForge.Infrastructure` contains a `ChaosForgeDbContextFactory : IDesignTimeDbContextFactory<ChaosForgeDbContext>` that uses a hardcoded local SQLite path for design-time operations.

### Migration applied but tests fail with schema errors
**Cause:** The in-memory SQLite used in tests is not running migrations — it uses `EnsureCreated()`.
**Fix:** Verify that `Application.Tests` and `Infrastructure.Tests` test fixtures call `dbContext.Database.EnsureCreated()` after the context is initialized. For Infrastructure tests that need migration fidelity, use `dbContext.Database.Migrate()` instead.

## References
- See ADR-008 for SQLite + EF Core design rationale
- See `conventions/toolchain.md` for the short-form command reference used in the implementation workflow
