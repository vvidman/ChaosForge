---
category: domain
area: "Agent System"
last_updated: "2026-03-29"
related_adr: [ADR-003, ADR-004]
---

# Agent System

## Overview
The agent system defines how AI agents are configured, instantiated, and tracked within a
project. `AgentSlot` is the configuration (role + allowed count); `AgentInstance` is a
running agent with identity and current status. Singleton roles may only have one slot and
one instance; multi-slot roles (Developer, Tester, Reviewer, TechnicalWriter) may have 1..N.

## Key Concepts

### AgentSlot
Project-level configuration declaring which roles are active and how many instances are allowed.
Created during the Setup phase before the project starts.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| ProjectId | Guid | |
| Role | AgentRole | |
| Count | int | max 1 for singleton roles; 1..N for multi-slot roles |

### AgentInstance
A live agent with an identity. Created when a slot is activated. Carries a persona name
for display purposes and tracks its current task assignment.

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| ProjectId | Guid | |
| Role | AgentRole | |
| PersonaName | string | display name shown in the UI |
| Status | AgentInstanceStatus | Idle / Working / Blocked / Finished |
| CurrentTaskId | Guid? | null when Idle or Finished |

### AgentRole
| Role | Singleton | Phase active | Output |
|---|---|---|---|
| BusinessAnalyst | YES | RequirementsPhase | URS |
| Architect | YES | ArchitecturePhase | SRS + WorkTasks |
| ScrumMaster | YES | SprintPlanning | Sprint plan |
| Developer | 1..N | Development | Task implementation |
| Tester | 1..N | Development | Unit tests + integration cases |
| Reviewer | 1..N | Development | Code review decision |
| TechnicalWriter | 1..N | Development | Documentation |

### AgentInstanceStatus
| Status | Meaning |
|---|---|
| Idle | Available to pick up a task |
| Working | Currently executing a TaskAttempt |
| Blocked | Waiting for a RevisionGate resolution |
| Finished | No more work in current phase |

## Business Rules
- `AgentSlot.Count` must be exactly 1 for `BusinessAnalyst`, `Architect`, `ScrumMaster`
- An `AgentInstance` may only be created if an `AgentSlot` exists for its role in the project
- `CurrentTaskId` must be null when `Status` is `Idle` or `Finished`
- `CurrentTaskId` must be non-null when `Status` is `Working`
- An `AgentInstance` may not pick up a new task while `Status = Working`
- `PersonaName` is immutable after the instance is created

## Boundaries
**In scope:** AgentSlot configuration, AgentInstance identity and status, role definitions, singleton constraints
**Out of scope:** how agents execute work (see `work-execution.md`), which LLM provider each role uses (see `llm-strategy.md` in architecture), BackgroundService polling mechanics (see `agent-design.md` in architecture)

## Related Areas
- `work-execution.md` — TaskAttempts that AgentInstances produce
- `project-lifecycle.md` — Setup phase where AgentSlots are configured

## References
- See ADR-003 for BackgroundService worker design
- See ADR-004 for ILLMProvider abstraction per role
- See `agent-design.md` (architecture) for execution mechanics and prompt injection
