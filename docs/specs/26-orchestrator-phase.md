---
category: specs
title: "Phase Orchestrator — Domain Event Notification Handlers"
branch: "orch-phase"
status: ready
date: "2026-04-12"
related_domain: [Project, RevisionGate, AgentInstance]
related_adr: [003-background-service-workers, 009-signalr-events]
---

# Feature Spec — Phase Orchestrator — Domain Event Notification Handlers

<!-- Reference this file in the implementation agent with: implement @docs/specs/26-orchestrator-phase.md -->

---

## Context

The agent workers are autonomous polling loops — they pick up work when it's available.
But something must react to gate resolutions and advance the project phase, and retire
agents whose phase is over. This spec implements two notification handlers:
`RevisionGateResolvedHandler` advances the project phase when a gate is accepted;
`ProjectStatusChangedHandler` retires agents from the previous phase.

Agent instance **creation** for the new phase is a separate responsibility handled in
spec 31. These two concerns are intentionally split: this spec handles transitions out
of a phase, spec 31 handles transitions into one.

Depends on: specs 22–25 (agents), spec 18 (dispatcher).

---

## Domain Impact

- New or modified entity: none
- New domain event: none (reacts to existing `RevisionGateResolvedEvent` and `ProjectStatusChangedEvent`)
- New interface: none

---

## Architecture Decisions

- Notification handlers live in `Application/Orchestration/`. They are MediatR
  `INotificationHandler<TEvent>` implementations — they run after `SaveChangesAsync`
  inside the `DomainEventDispatcher.DispatchAsync` call chain.
- Handlers are `internal sealed`.
- Handlers must NOT call `SaveChangesAsync` themselves — the UnitOfWork is already
  committed by the time the event fires. Commands they send go through MediatR; each
  command handler manages its own `SaveChangesAsync`.
- Handlers are registered automatically by `AddApplication()` via MediatR assembly scan.

### RevisionGateResolvedHandler

Reacts to `RevisionGateResolvedEvent`:
- `Action == Reject`: no-op — the agent worker retries on the next poll cycle.
- `Action == Accept` or `Action == EditAndAccept`: fetch the gate to get `ProjectId`
  and `Type`, then send `TransitionProjectCommand` for the appropriate next phase:

  | RevisionGateType | Next phase |
  |---|---|
  | AfterBA | ArchitecturePhase |
  | AfterArchitect | SprintPlanning |
  | AfterScrumMaster | Development |

- If `EditAndAccept`: also call `IButterflyService.PropagateAsync` (spec 28) **before**
  the phase transition, so downstream data is updated before the next phase agents activate.

### ProjectStatusChangedHandler

Reacts to `ProjectStatusChangedEvent`. Responsibility: **retire** agents from the phase
that just ended.

- Fetch all `AgentInstance` records for the project.
- Determine which roles are active in `NewStatus`:

  | Phase | Active roles |
  |---|---|
  | RequirementsPhase | BusinessAnalyst |
  | ArchitecturePhase | Architect |
  | SprintPlanning | ScrumMaster |
  | Development | Developer, Tester, Reviewer, TechnicalWriter |
  | Completed | (none) |
  | Setup | (none) |

- Send `MarkAgentFinishedCommand` for every instance whose role is **not** in the active
  set and whose status is not already `Finished`.
- Do NOT create instances here — that is `AgentInstanceActivationHandler` in spec 31.

---

## Implementation Scope — What must be done

- [ ] Create `Application/Orchestration/RevisionGateResolvedHandler.cs`:
  - Implements `INotificationHandler<RevisionGateResolvedEvent>`
  - Injects `IMediator`, `IRevisionGateRepository`, `IButterflyService`
  - On Reject: log and return
  - On Accept/EditAndAccept: if EditAndAccept → call `IButterflyService.PropagateAsync`
  - Always on Accept/EditAndAccept: send `TransitionProjectCommand` for the next phase

- [ ] Create `Application/Orchestration/ProjectStatusChangedHandler.cs`:
  - Implements `INotificationHandler<ProjectStatusChangedEvent>`
  - Injects `IMediator`, `IAgentInstanceRepository`
  - Fetches all instances for `ProjectId`
  - Sends `MarkAgentFinishedCommand` for every instance with a role not in the new
    phase's active set and status not already `Finished`

- [ ] Run `dotnet build` — zero warnings, zero errors
- [ ] Run `dotnet test` — all existing tests pass

---

## Out of Scope — What must NOT be done

- Do not create new `AgentInstance` records here — that is spec 31
- Do not trigger SignalR notifications here — that is spec 30
- Do not implement ButterflyService logic here — only call it, spec 28 implements it

---

## Test Expectations

- Unit tests required for:
  - `RevisionGateResolvedHandler`: Accept on AfterBA → `TransitionProjectCommand` to
    `ArchitecturePhase` sent; `IButterflyService` NOT called
  - `RevisionGateResolvedHandler`: EditAndAccept on AfterArchitect → `IButterflyService`
    called first, then `TransitionProjectCommand` to `SprintPlanning`
  - `RevisionGateResolvedHandler`: Reject → no command sent, no butterfly call
  - `ProjectStatusChangedHandler`: transition to ArchitecturePhase → BusinessAnalyst
    instances marked Finished, Architect instances untouched
  - `ProjectStatusChangedHandler`: transition to Completed → all instances marked Finished
- Edge cases to cover: no AgentInstances exist for project → handler completes without error

---

## Open Questions

- None. Instance creation on phase entry is fully covered by spec 31.
