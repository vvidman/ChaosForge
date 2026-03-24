# ChaosForge – Claude Code Project Memory

## Project Overview

ChaosForge is a **multi-agent AI software development team simulator** built in .NET 10.
A human defines a project with Use Cases. AI agents (BA, Architect, SM, Developer, Tester,
Reviewer, Technical Writer) work through a full Scrum-like workflow autonomously.
The human acts as a judge: can Accept, Edit \& Accept, or Reject at key revision gates.
The system embraces non-determinism — chaos is by design.

See @docs/architecture.md for full architecture decisions.
See @docs/domain-model.md for entity and state machine details.
See @docs/workflow.md for the complete agent workflow.

\---

## Tech Stack

* **Backend**: .NET 10, ASP.NET Core Web API
* **Frontend**: React (Vite + TypeScript)
* **Real-time**: SignalR
* **ORM**: Entity Framework Core + SQLite
* **LLM Local**: LlamaSharp (CPU-only, no GPU)
* **LLM Online**: Groq API (free tier), OpenAI-compatible
* **Architecture**: Clean Architecture + CQRS (MediatR)
* **Testing**: xUnit, FluentAssertions, NSubstitute

\---

## Solution Structure

```
ChaosForge.slnx
├── src/
│   ├── ChaosForge.Domain          ← Entities, domain events, interfaces. ZERO external deps.
│   ├── ChaosForge.Application     ← CQRS Commands/Queries via MediatR, use cases
│   ├── ChaosForge.Infrastructure  ← EF Core, LlamaSharp, Groq, BackgroundService workers
│   ├── ChaosForge.API             ← ASP.NET Core controllers, SignalR Hub, thin layer only
│   └── ChaosForge.Web             ← React frontend (Vite + TypeScript)
└── tests/
    ├── ChaosForge.Domain.Tests
    ├── ChaosForge.Application.Tests
    └── ChaosForge.Infrastructure.Tests
```

\---

## Architecture Rules — IMPORTANT

* **Domain layer has ZERO dependencies** on any other project or NuGet package (except primitives)
* **Application layer** depends ONLY on Domain. Never on Infrastructure.
* **Dependency Inversion**: Infrastructure implements interfaces defined in Domain/Application
* **Controllers are thin**: no business logic in API layer, only dispatch Commands/Queries
* **CQRS**: every operation is either a Command (write) or a Query (read), never mixed
* Never use `static` classes for business logic
* Never bypass the domain model — all state changes go through domain entities and events

**SOLID principles are mandatory** — see @.claude/rules/solid-principles.md for detailed
rules with ChaosForge-specific examples. Every class must satisfy the SOLID checklist
before it is considered done.

\---

## Git Management — IMPORTANT

**Claude Code never runs any Git commands.**

The human manages all version control operations:

* `git add`, `git commit`, `git push`, `git merge`, `git branch` — all done by the human
* Claude Code works exclusively on local working copy files
* When a task is complete, Claude Code reports what files were created or modified
* The human reviews the changes and decides when and what to commit

Do NOT run `git commit`, `git add`, `git push`, or any other git command unless
explicitly asked by the human for a one-off specific reason.

\---

## Desktop + Claude Code Workflow — IMPORTANT

This project uses two complementary tools. They are not interchangeable — each has a clear role.

### Tool responsibilities

|Claude.ai Desktop|Claude Code|
|-|-|
|Architecture decisions|Multi-file implementation|
|Feature planning and spec writing|Running and fixing tests|
|Code review and refactoring discussion|Autonomous "build it" tasks|
|Quick questions, design iteration|Full repo context operations|

### The 3-step workflow

```
Step 1 — Design (Desktop)
  Discuss and agree on architecture, write the spec
  → docs/specs/feature-name.md
  → docs/decisions/ADR-XXX.md  (if an architecture decision was made)

Step 2 — Implement (Claude Code)
  "implement @docs/specs/feature-name.md"
  Write code, run tests, report changed files

Step 3 — Review (Desktop)
  Human pastes code/diff into Desktop, discusses and refines
  → feedback comes back to Claude Code as /review-fix
  → human commits when satisfied
```

### Rules for Claude Code in this workflow

* Always check `docs/specs/` and `docs/decisions/` before starting implementation
* Specs and ADRs written in Desktop are **final decisions** — do not deviate without flagging a conflict
* Do not make architecture decisions independently — propose them and wait for confirmation
* Do not self-review your own output — review happens in Desktop
* When implementation is complete, report: files created, files modified, tests added, manual steps needed
* Do NOT run any git commands — the human handles all commits

### When NO spec exists

If asked to implement something non-trivial that touches multiple layers, ask:
"Should I create a spec draft for Desktop review first, or proceed?"
For small changes (single-layer fix, rename, config tweak) — proceed directly.

\---

## Coding Standards

See @.claude/rules/coding-conventions.md for the full coding conventions.

Key non-negotiables:

* C# 13, nullable reference types enabled — trust the type system, no redundant null checks
* Async all the way — no `.Result` or `.Wait()` calls, always pass `CancellationToken`
* No trailing whitespace, final return on its own line
* `is null` / `is not null` — never `== null` or `!= null`
* `nameof` instead of string literals for member names

\---

## Domain Key Concepts

### Agent Roles

|Role|Singleton|Responsibility|
|-|-|-|
|BusinessAnalyst|YES|Use Cases → URS|
|Architect|YES|URS → SRS → Tasks|
|ScrumMaster|YES|Backlog prioritization + Sprint planning|
|Developer|1..N|Task implementation + refinement|
|Tester|1..N|Code analysis, unit test generation, integration test cases|
|Reviewer|1..N|Code review, accept or reject|
|TechnicalWriter|1..N|Documentation from finished code|

### Workflow Phases (ProjectStatus)

`Setup → RequirementsPhase → ArchitecturePhase → SprintPlanning → Development → Completed`

### Revision Gates

After BA, Architect, SM — human can: `Accept | EditAndAccept | Reject`
Reject requires a mandatory reason. EditAndAccept modifies downstream inputs (butterfly effect).

### Task Lifecycle (WorkTaskStatus)

`Backlog → InProgress → InReview → InTesting → InDocumentation → Done`
Reviewer or Tester can reject back to Backlog with notes.

### TaskAttempt

Every dev/review/test cycle creates a new TaskAttempt record.
Each attempt receives the previous attempt output + rejection notes as prompt context.

\---

## LLM Provider Strategy

```
ILLMProvider (Domain interface)
├── LlamaSharpProvider       → local CPU inference
├── GroqProvider             → free online, Llama 3.3 70B for complex roles
└── OpenAICompatibleProvider → generic fallback
```

Role → Provider mapping is configurable per project at runtime.
Complex roles (BA, Arch, SM) → Groq. Repetitive roles (Dev, Test, Review, TW) → LlamaSharp.

\---

## Build and Run Commands

```bash
# Restore all dependencies
dotnet restore

# Build solution
dotnet build

# Run API
dotnet run --project src/ChaosForge.API

# Run all tests
dotnet test

# Run frontend
cd src/ChaosForge.Web \&\& npm install \&\& npm run dev

# EF Core migrations
dotnet ef migrations add <Name> --project src/ChaosForge.Infrastructure --startup-project src/ChaosForge.API
dotnet ef database update --project src/ChaosForge.Infrastructure --startup-project src/ChaosForge.API
```

\---

## What NOT to Do — IMPORTANT

* NEVER run git commands (see Git Management section above)
* NEVER put business logic in controllers or SignalR hubs
* NEVER reference Infrastructure from Domain or Application
* NEVER call LLM providers directly from Application layer — always through ILLMProvider
* NEVER make AgentWorkerService aware of HTTP/SignalR — use domain events
* NEVER use raw SQL — EF Core only
* NEVER commit secrets or API keys — use dotnet user-secrets or environment variables
* NEVER use `.Result` or `.Wait()` on async calls

