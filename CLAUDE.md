# ChaosForge – Claude Code Project Memory

## What is this project
ChaosForge is a **multi-agent AI software development team simulator** built in .NET 10.
A human defines a project with Use Cases. AI agents (BA, Architect, SM, Developer, Tester,
Reviewer, Technical Writer) work through a full Scrum-like workflow autonomously.
The human acts as a judge: can Accept, Edit \& Accept, or Reject at key revision gates.
The system embraces non-determinism — chaos is by design.

## Tech Stack
* **Backend**: .NET 10, ASP.NET Core Web API
* **Frontend**: React (Vite + TypeScript)
* **Real-time**: SignalR
* **ORM**: Entity Framework Core + SQLite
* **LLM Local**: LlamaSharp (CPU-only, no GPU)
* **LLM Online**: Groq API (free tier), OpenAI-compatible
* **Architecture**: Clean Architecture + CQRS (MediatR)
* **Testing**: xUnit, FluentAssertions, NSubstitute

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

## Coding Style
Non-trivial conventions: see `docs/conventions/README.md`

Key non-negotiables:
* C# 13, nullable reference types enabled — trust the type system, no redundant null checks
* Async all the way — no `.Result` or `.Wait()` calls, always pass `CancellationToken`
* No trailing whitespace, final return on its own line
* `is null` / `is not null` — never `== null` or `!= null`
* `nameof` instead of string literals for member names

## Permanent Prohibitions
* NEVER put business logic in controllers or SignalR hubs
* NEVER reference Infrastructure from Domain or Application
* NEVER call LLM providers directly from Application layer — always through ILLMProvider
* NEVER make AgentWorkerService aware of HTTP/SignalR — use domain events
* NEVER use raw SQL — EF Core only
* NEVER commit secrets or API keys — use dotnet user-secrets or environment variables
* NEVER use `.Result` or `.Wait()` on async calls

---

## Contributor Navigation Protocol
> This applies to both AI assistants and human contributors.

Before starting any task, read `docs/README.md` to discover relevant knowledge areas.
Then load only the category manifest and specific files relevant to the current task.

### Trigger Table
| Situation                            | Load first                       | Priority if conflict    |
|--------------------------------------|----------------------------------|-------------------------|
| Writing or reviewing code            | `docs/conventions/README.md`     | ADR > Conventions       |
| Making architectural decisions       | `docs/adr/README.md`             | ADR is authoritative    |
| Applying a design principle          | `docs/architecture/README.md`    | Architecture > Conventions |
| Working on a feature or domain area  | `docs/domain/README.md`          | Domain > Conventions    |
| Implementing a specified feature     | `docs/specs/README.md`           | Spec defines the scope  |
| Touching build, CI/CD, or tooling    | `docs/toolchain/README.md`       | Toolchain is isolated   |

### Conflict Resolution Priority
**ADR > Domain > Architecture > Conventions > Toolchain**

Specs are not standing rules — they define intent for a specific feature and are consumed during implementation only.

---

## File Conventions in `/docs`
- `README.md` files are **category manifests** — use them for navigation
- `_` prefixed files (e.g. `_template.md`) are **authoring templates** — never load as knowledge, never reference as a source
- All other `.md` files are **loadable knowledge documents**

---

## Implementation Workflow — MANDATORY

Every non-trivial implementation follows these steps in order. Do not skip steps.

1. **Branch** — create branch from spec `branch` frontmatter field.
2. **Plan** — read the spec from `docs/specs/`, produce a numbered plan. Wait for approval.
3. **Implement** — follow the spec. Flag conflicts instead of deviating.
4. **License** — every new source file requires the license header. See `docs/conventions/csharp.md`.
5. **Test** — generate or update unit tests. See `docs/conventions/testing.md`.
6. **Build** — `dotnet build` must pass with zero errors.
7. **Finalize spec** — after successful build, update the feature spec file:
   - Set `status: done` in the frontmatter
   - Check off all completed items in the Implementation Scope checklist
   - Update the `docs/specs/README.md` frontmatter entry and table row: set status to `done`
8. **Commit + PR** — commit, open PR to `dev`. Report files created/modified, tests added, manual steps.

For coding standards: `docs/conventions/`
For build commands: load `docs/conventions/toolchain.md` before running any dotnet command.

Never run git commands outside this flow. Never self-review.