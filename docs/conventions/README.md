---
category: conventions
last_updated: "2026-03-29"
documents:
  - file: csharp.md
    covers: ["C#", "nullability", "naming", "async", "records", "formatting", "license header", "static", "CancellationToken"]
  - file: testing.md
    covers: ["xUnit", "FluentAssertions", "NSubstitute", "unit tests", "mocking", "AAA", "test naming", "in-memory SQLite", "Theory", "Fact"]
  - file: git.md
    covers: ["branch naming", "commit messages", "PR", "pull request", "squash merge", "secrets", "dev branch", "feat", "fix", "chore"]
  - file: toolchain.md
    covers: ["dotnet build", "dotnet test", "dotnet run", "EF Core", "migrations", "npm", "user-secrets", "environment variables", "dotnet restore"]
---

# Conventions

Coding standards and toolchain rules for all contributors — human and AI alike.
When a convention conflicts with an ADR, the ADR takes precedence.

## Documents

### `csharp.md`
C# 13 coding style: nullability, naming, async rules, record and type usage, formatting,
and the mandatory license header requirement.
Load when: writing or reviewing any C# source file.

### `testing.md`
xUnit test structure, FluentAssertions patterns, NSubstitute usage, and what each test
layer must cover. Includes naming convention and isolation rules.
Load when: writing tests, reviewing test quality, or adding a new test project.

### `git.md`
Branch naming (sourced from spec frontmatter), commit message format, PR structure and
description requirements, and merge strategy.
Load when: creating a branch, writing a commit, or opening a PR.

### `toolchain.md`
Exact `dotnet` and `npm` commands for build, test, run, and EF Core migrations.
Secret management and environment variable patterns.
Load when: running the project, adding a migration, or setting up a new environment.

---

## Adding a New Convention Document
Copy `_template.md` to a new file named after the topic (e.g. `error-handling.md`).
Add the new file to the `documents` list in this README's frontmatter.
