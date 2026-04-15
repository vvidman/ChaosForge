---
category: specs
title: "Agent Instance Activation Handler"
branch: "agent-activate"
status: done
date: "2026-04-12"
related_domain: [AgentInstance, AgentSlot, Project]
related_adr: [003-background-service-workers]
---

# Feature Spec — Agent Instance Activation Handler

<!-- Reference this file in the implementation agent with: implement @docs/specs/31-agent-instance-activation.md -->

---

## Context

When a project enters a new phase, the appropriate `AgentInstance` records must exist and
be in `Idle` status for the agent workers to pick them up. Without automatic activation,
the operator would need to manually call `POST /api/agent-instances` for every role before
each phase — an error-prone setup that breaks the demo flow. This spec introduces
`AgentInstanceActivationHandler`, a `INotificationHandler<ProjectStatusChangedEvent>` that
automatically creates `AgentInstance` records from the project's `AgentSlot` configuration
when a new phase begins.

Spec 26's `ProjectStatusChangedHandler` handles the **exit** side (retiring old agents).
This spec handles the **entry** side (activating new agents). They are both triggered by
the same event but have separate, complementary responsibilities.

Depends on: spec 26 (event infrastructure), spec 16 (`CreateAgentInstanceCommand`).

---

## Domain Impact

- New or modified entity: none
- New domain event: none (reacts to `ProjectStatusChangedEvent`)
- New interface: none

---

## Architecture Decisions

- `AgentInstanceActivationHandler` lives in `Application/Orchestration/`.
- It is `internal sealed`, implements `INotificationHandler<ProjectStatusChangedEvent>`.
- Registered automatically by MediatR assembly scan in `AddApplication()`.
- Activation logic:
  1. Determine which roles are active in `NewStatus` (same phase-to-roles mapping as spec 26).
  2. Fetch all `AgentSlot` records for the project via `IAgentSlotRepository.GetByProjectIdAsync`.
  3. For each slot whose `Role` is in the active set:
     - Fetch existing `AgentInstance` records for this project and role via
       `IAgentInstanceRepository.GetByProjectIdAsync` (filter in memory by role).
     - Count how many are NOT `Finished` — these are still usable.
     - If the count is less than `slot.Count`: create the missing instances via
       `CreateAgentInstanceCommand`. Number to create = `slot.Count - existingActiveCount`.
  4. Persona name for new instances: `"{Role}-{shortGuid}"` where `shortGuid` is the first
     8 characters of `Guid.NewGuid().ToString("N")`. Example: `"Developer-3f8a1b2c"`.
     This is sufficient for display purposes in the demo.
- Idempotency: if instances already exist from a previous run (e.g. after a gate rejection
  and retry), they are reused — not duplicated. The count check prevents over-creation.
- Instances created with `Finished` status from a previous phase are NOT reused — the
  filter for "usable" is `Status != Finished`.
- If `NewStatus` has no active roles (Setup, Completed): no instances are created.
- If no `AgentSlot` exists for a role that should be active: no instance is created for
  that role, and a warning is logged. The operator must configure slots during Setup.

---

## Implementation Scope — What must be done

- [x] Create `Application/Orchestration/AgentInstanceActivationHandler.cs`:
  - Implements `INotificationHandler<ProjectStatusChangedEvent>`
  - Injects `IMediator`, `IAgentSlotRepository`, `IAgentInstanceRepository`
  - Implements activation logic as described above
  - Logs a warning when no `AgentSlot` is found for an expected active role
  - Persona name generation: `$"{role}-{Guid.NewGuid().ToString("N")[..8]}"`

- [x] Verify no duplicate instance creation: run unit test with pre-existing `Idle` instances
  — confirms count check prevents over-creation

- [x] Run `dotnet build` — zero warnings, zero errors
- [x] Run `dotnet test` — all existing tests pass

---

## Out of Scope — What must NOT be done

- Do not create `AgentSlot` records automatically — slot configuration is a deliberate
  human step during the Setup phase
- Do not start the workers from this handler — workers are hosted services that poll
  continuously; they will pick up the new instances on the next cycle
- Do not retire instances here — that is `ProjectStatusChangedHandler` in spec 26

---

## Test Expectations

- Unit tests required for:
  - Transition to `RequirementsPhase`: one `BusinessAnalyst` slot with `Count = 1`,
    no existing instances → exactly one `CreateAgentInstanceCommand` sent
  - Transition to `Development`: one `Developer` slot with `Count = 2`, one existing
    `Idle` Developer instance → exactly one more `CreateAgentInstanceCommand` sent
  - Transition to `Development`: slot exists but all instances already `Idle` and count
    matches → zero `CreateAgentInstanceCommand` calls
  - Transition to `Development`: `Finished` instances do not count toward existing active →
    new instances created to fill the slot
  - Transition to `Completed`: no active roles → zero `CreateAgentInstanceCommand` calls
  - No `AgentSlot` found for active role → warning logged, no command sent, no exception
- Edge cases to cover: transition to same phase (should not happen by domain rules, but
  handler must not crash if it does — idempotent behaviour holds)

---

## Open Questions

- None.
