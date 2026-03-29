---
category: conventions
topic: "Toolchain and Build Commands"
last_updated: "2026-03-29"
related_adr: [ADR-008]
---

# Toolchain and Build Commands

Reference for all build, run, test, and migration commands. Use these exact invocations —
do not shorten or modify flags without a documented reason.

---

## Prerequisites

| Tool | Version | Notes |
|---|---|---|
| .NET SDK | 10.x | `dotnet --version` to verify |
| Node.js | 20+ | frontend only |
| EF Core tools | latest | `dotnet tool install -g dotnet-ef` |

---

## Backend

```bash
# Restore all dependencies
dotnet restore

# Build entire solution — must pass before any commit
dotnet build

# Run API (development)
dotnet run --project src/ChaosForge.API

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run a single test project
dotnet test tests/ChaosForge.Application.Tests
```

---

## Frontend

```bash
# Install dependencies (first time or after package.json changes)
cd src/ChaosForge.Web && npm install

# Run dev server
npm run dev

# Build for production
npm run build
```

---

## EF Core Migrations

```bash
# Add a new migration
dotnet ef migrations add <MigrationName> \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API

# Apply migrations to local database
dotnet ef database update \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API

# List applied migrations
dotnet ef migrations list \
  --project src/ChaosForge.Infrastructure \
  --startup-project src/ChaosForge.API
```

Migration naming convention: `PascalCase`, descriptive, no timestamp prefix.
Examples: `AddRevisionGateEntity`, `AddTaskAttemptReviewNote`, `SeedAgentRoles`

---

## Secrets and Configuration

```bash
# Set a user secret (never commit secrets to source)
dotnet user-secrets set "Groq:ApiKey" "<value>" --project src/ChaosForge.API

# List current user secrets
dotnet user-secrets list --project src/ChaosForge.API
```

Environment variable override pattern (production / CI):
`ChaosForge__Groq__ApiKey=<value>` — double underscore maps to nested JSON keys.

---

## Rules

- **Must**: `dotnet build` must succeed with zero errors before any commit
- **Must**: `dotnet test` must pass before opening a PR
- **Must**: migrations are named descriptively — never `Migration1` or auto-generated timestamps
- **Must**: secrets go in `user-secrets` or environment variables — never `appsettings.json`
- **?** Does the migration name describe exactly what schema change it makes?
- **?** Is there any hardcoded connection string or API key in any `.json` or `.cs` file?

## Anti-patterns

### Committing with build warnings treated as errors
The solution is configured to treat specific warnings as errors. A passing build means
zero errors — warnings should be reviewed, not suppressed with `#pragma`.

### Manual database edits
Never modify the SQLite database file directly. All schema changes go through EF Core migrations
so the change is versioned and reproducible.

## References
- See ADR-008 for SQLite + EF Core design rationale
- See `git.md` for commit and PR requirements that depend on a passing build
