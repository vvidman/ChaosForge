# ChaosForge – Agent Workflow Reference

## Full Workflow

```
[1] PROJECT SETUP (Human)
    ├── Create project (name, description, deadline)
    ├── Configure agent roles and counts
    └── Define Use Cases (user-facing feature descriptions)

[2] REQUIREMENTS PHASE (BusinessAnalyst agent)
    └── Use Cases → URS items
    ⛔ REVISION GATE: AfterBA
        ├── Accept          → proceed to Architecture Phase
        ├── EditAndAccept   → human edits URS, butterfly effect downstream
        └── Reject + reason → BA reruns with rejection context

[3] ARCHITECTURE PHASE (Architect agent)
    ├── URS → SRS items
    └── SRS → WorkTask breakdown
    ⛔ REVISION GATE: AfterArchitect
        ├── Accept          → proceed to Sprint Planning
        ├── EditAndAccept   → human edits SRS/tasks
        └── Reject + reason → rollback target:
            ├── Architect start (redo SRS and tasks)
            └── BA start (redo URS first)

[4] SPRINT PLANNING PHASE (ScrumMaster agent)
    ├── Prioritize backlog (deadline-aware)
    └── Generate sprint plan (N sprints, task assignments)
    ⛔ REVISION GATE: AfterScrumMaster
        ├── Accept          → Development Phase begins
        ├── EditAndAccept   → human adjusts sprint plan
        └── Reject + reason → rollback target:
            ├── SM start
            ├── Architect start
            └── BA start

[5] DEVELOPMENT PHASE (parallel agents)

    Task lifecycle:
    ┌─────────────────────────────────────────────────────┐
    │  Backlog                                            │
    │    └─► InProgress      ← Developer picks up task   │
    │          └─► InReview  ← Developer marks done      │
    │                │                                    │
    │         ┌──────┴──────┐                            │
    │         ▼             ▼                            │
    │     InTesting      Backlog  ← Reviewer rejects     │
    │         │           (+ review note, prev output    │
    │         │            visible to next Developer)    │
    │  ┌──────┴──────┐                                   │
    │  ▼             ▼                                   │
    │ InDocumentation  Backlog  ← Tester rejects         │
    │       │          (+ test note)                     │
    │       ▼                                            │
    │      Done ← TechnicalWriter documents              │
    └─────────────────────────────────────────────────────┘

[6] PROJECT COMPLETED
    └── All tasks Done → summary report generated
```

---

## Revision Gate: Butterfly Effect

When a human selects **EditAndAccept** at any revision gate:

- The modified output replaces the agent output as the canonical version
- All downstream entities derived from the modified output are flagged for re-evaluation
- Example: editing a URS item may invalidate related SRS items and tasks
- The `ButterflyService` handles propagation of these invalidation events

This is intentional — a single human edit can cascade into significant downstream changes,
reflecting the butterfly effect the system is named after.

---

## Task Attempt Context

When a Developer picks up a task that was previously rejected:

```
Prompt context includes:
  1. Original task description (from SRS)
  2. Previous attempt output (what was written before)
  3. Rejection note (what was wrong, from Reviewer or Tester)
  4. Role persona (Developer)
```

This gives the agent full context for refinement without repeating mistakes.

---

## Agent Role Summary

| Role | Phase | Input | Output |
|---|---|---|---|
| BusinessAnalyst | Requirements | Use Cases | URS items |
| Architect | Architecture | URS items | SRS items + WorkTasks |
| ScrumMaster | Sprint Planning | WorkTasks + deadline | Sprint plan |
| Developer | Development | SRS WorkTask (+ prev attempt) | Implementation |
| Reviewer | Development | Developer output | Approved or Rejected + note |
| Tester | Development | Reviewer-approved output | Passed or Failed + note |
| TechnicalWriter | Documentation | Tested output | Technical documentation |

---

## Parallelism Model

- BA, Architect, SM run **sequentially** — each phase gates the next
- Developer, Reviewer, Tester, TechnicalWriter run **in parallel** within the Development Phase
- Multiple Developers can hold different tasks simultaneously
- A single task is held by at most one agent at a time (`InProgress` = locked)
- AgentWorkerService polls for available tasks per role — no central coordinator
