---
category: adr
last_updated: "2026-03-29"
documents:
  - file: 001-clean-architecture.md
    title: "Clean Architecture layer model"
    status: accepted
    date: "2026-03-29"
  - file: 002-cqrs-mediatr.md
    title: "CQRS dispatch via MediatR"
    status: accepted
    date: "2026-03-29"
  - file: 003-background-service-workers.md
    title: "BackgroundService for agent workers"
    status: accepted
    date: "2026-03-29"
  - file: 004-illmprovider-abstraction.md
    title: "ILLMProvider abstraction for all LLM calls"
    status: accepted
    date: "2026-03-29"
  - file: 005-revision-gate-entity.md
    title: "RevisionGate as a first-class domain entity"
    status: accepted
    date: "2026-03-29"
  - file: 006-task-attempt-per-cycle.md
    title: "TaskAttempt record per dev/review/test cycle"
    status: accepted
    date: "2026-03-29"
  - file: 007-llamasharp-vs-ollama.md
    title: "LlamaSharp for local inference over Ollama HTTP"
    status: accepted
    date: "2026-03-29"
  - file: 008-sqlite-efcore.md
    title: "SQLite with EF Core for persistence"
    status: accepted
    date: "2026-03-29"
  - file: 009-signalr-events.md
    title: "SignalR for real-time agent event delivery"
    status: accepted
    date: "2026-03-29"
  - file: 010-revision-gate-type-naming.md
    title: "RevisionGateType naming: phase-centric over agent-centric"
    status: accepted
    date: "2026-04-12"	
---

# Architecture Decision Records

ADRs document significant decisions made about the architecture and design of ChaosForge.
They are the **authoritative source** — if an ADR conflicts with a convention or architecture
principle, the ADR wins.

## Status Vocabulary
- `proposed` — under discussion, not yet binding
- `accepted` — active, must be followed
- `deprecated` — no longer applies, kept for historical reference
- `superseded-by` — replaced by a newer ADR (reference provided in the document)

## Documents

| # | Title | Status | Date |
|---|-------|--------|------|
| 001 | Clean Architecture layer model | accepted | 2026-03-29 |
| 002 | CQRS dispatch via MediatR | accepted | 2026-03-29 |
| 003 | BackgroundService for agent workers | accepted | 2026-03-29 |
| 004 | ILLMProvider abstraction for all LLM calls | accepted | 2026-03-29 |
| 005 | RevisionGate as a first-class domain entity | accepted | 2026-03-29 |
| 006 | TaskAttempt record per dev/review/test cycle | accepted | 2026-03-29 |
| 007 | LlamaSharp for local inference over Ollama HTTP | accepted | 2026-03-29 |
| 008 | SQLite with EF Core for persistence | accepted | 2026-03-29 |
| 009 | SignalR for real-time agent event delivery | accepted | 2026-03-29 |
| 010 | RevisionGateType naming: phase-centric over agent-centric | accepted | 2026-04-12 |

---

## Adding a New ADR
Copy `_template.md` to a new file with an incremented number and short slug: `011-short-description.md`.
Update the `documents` table and frontmatter list in this README.
Add `related_principles` to the frontmatter referencing the relevant architecture category documents.
