---
category: architecture
last_updated: "2026-03-29"
documents:
  - file: clean-architecture.md
    covers: ["layers", "dependency rule", "dependency inversion", "boundaries", "ports and adapters", "DI registration"]
  - file: solid.md
    covers: ["SRP", "OCP", "LSP", "ISP", "DIP", "single responsibility", "open closed", "interface segregation", "dependency inversion"]
  - file: cqrs.md
    covers: ["CQRS", "MediatR", "commands", "queries", "DTOs", "FluentValidation", "handlers"]
  - file: agent-design.md
    covers: ["agent roles", "BackgroundService", "AgentWorkerService", "RevisionGate", "TaskAttempt", "ButterflyService", "workflow phases", "task lifecycle"]
  - file: llm-strategy.md
    covers: ["ILLMProvider", "LlamaSharp", "Groq", "OpenAICompatible", "provider mapping", "role assignment"]
---

# Architecture Principles

This category defines the design principles and structural patterns applied in ChaosForge,
and how they are interpreted in this specific codebase.

**Conflict resolution**: ADR > Domain > Architecture > Conventions > Toolchain.
When an architecture principle conflicts with a convention, the principle takes precedence.
When an ADR conflicts with an architecture principle, the ADR takes precedence.

---

## Documents

### `clean-architecture.md`
Layer structure, dependency direction, and how Domain, Application, and Infrastructure concerns
are separated. DI registration pattern included.
Load when: reasoning about where code belongs, reviewing layer boundaries, questioning dependency direction.

### `solid.md`
How the five SOLID principles are applied and enforced in ChaosForge, with project-specific
rules and checking questions for each principle.
Load when: designing new classes or interfaces, reviewing OO design decisions, conducting code review.

### `cqrs.md`
Command/Query split via MediatR, DTO conventions, and FluentValidation requirements.
Load when: adding new commands or queries, reviewing handler design, questioning read/write separation.

### `agent-design.md`
Agent role definitions, `BackgroundService` worker pattern, `TaskAttempt` lifecycle,
`RevisionGate` logic, and the `ButterflyService` downstream propagation model.
Load when: working on any agent worker, implementing revision gates, reasoning about task or workflow state.

### `llm-strategy.md`
`ILLMProvider` abstraction, the three provider implementations, and the role-to-provider
mapping strategy.
Load when: adding or changing an LLM provider, configuring role assignments, reviewing DI registration.

---

## Adding a New Architecture Document
Copy `_template.md` to a new file named after the principle or pattern (e.g. `event-sourcing.md`).
Add the new file to the `documents` list in this README's frontmatter.
