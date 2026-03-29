---
category: domain
area: "Project Lifecycle"
last_updated: "2026-03-29"
related_adr: [ADR-001]
---

# Project Lifecycle

## Overview
`Project` is the aggregate root of the entire ChaosForge domain. Every agent, task, sprint,
and revision gate belongs to a project. The project's `Status` field drives which operations
are permitted at any point in time — it is the system's top-level state machine.

## Key Concepts

### Project
The root entity. Owns all other aggregates either directly or transitively.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| Name | string | |
| Description | string | |
| Status | ProjectStatus | state machine root — drives permitted operations |
| CreatedAt | DateTime | |
| Deadline | DateTime? | used by ScrumMaster for sprint planning |

### ProjectStatus state machine
```
Setup → RequirementsPhase → ArchitecturePhase → SprintPlanning → Development → Completed
```

Each phase unlocks specific agent roles and operations:

| Status | Active agents | Permitted operations |
|---|---|---|
| Setup | — | configure AgentSlots, define UseCases |
| RequirementsPhase | BusinessAnalyst | generate URS, open AfterBA gate |
| ArchitecturePhase | Architect | generate SRS + WorkTasks, open AfterArchitect gate |
| SprintPlanning | ScrumMaster | prioritize backlog, plan sprint, open AfterScrumMaster gate |
| Development | Developer, Tester, Reviewer, TechnicalWriter | task execution cycle |
| Completed | — | read-only |

## Business Rules
- A project may only advance to the next phase after the corresponding `RevisionGate` is resolved with `Accept` or `EditAndAccept`
- `Deadline` is optional at creation but must be set before `SprintPlanning` begins
- Phase transitions are irreversible — there is no "go back to previous phase" operation
- A project in `Completed` status is fully read-only; no entities under it may be modified

## Boundaries
**In scope:** project identity, phase state machine, phase transition rules, deadline
**Out of scope:** agent configuration (see `agent-system.md`), sprint contents (see `work-execution.md`), revision gate logic (see `revision-gate.md`)

## Related Areas
- `revision-gate.md` — gates that unlock phase transitions
- `agent-system.md` — AgentSlot configuration that happens during Setup
- `work-execution.md` — Sprint and WorkTask that become active in Development

## References
- See ADR-001 for the Clean Architecture layer model
- See `agent-design.md` (architecture) for the workflow phases in execution context
