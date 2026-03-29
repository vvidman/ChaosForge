---
category: adr
id: "005"
title: "RevisionGate as a first-class domain entity"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [agent-design, clean-architecture]
---

# ADR-005: RevisionGate as a first-class domain entity

## Status
`accepted`

## Context
At three points in the workflow (after BA, Architect, ScrumMaster output), a human must
review and decide: Accept, EditAndAccept, or Reject. EditAndAccept must propagate changes
downstream (ButterflyService). The original agent output, human edits, decision, and
rejection reason all need to be stored for audit and for prompt context in retry cycles.

## Decision
`RevisionGate` is a domain entity with its own identity, lifecycle, and relationships.
It stores: agent output, human-edited version (if any), decision enum, and mandatory
rejection reason. `EditAndAccept` triggers a `ButterflyService` domain event. The gate
is not a property of a task — it is an independent entity referenced by the workflow phase.

## Consequences

**Positive:**
- Full audit trail: what the agent produced, what the human changed, and why
- ButterflyService downstream propagation is a clean domain event — no special-casing in handlers
- Rejection reason is a first-class field, not an optional string on a task

**Trade-offs:**
- More entities to persist and query
- Gate lifecycle must be explicitly managed (opened, resolved) — cannot be inferred from task status alone

## Alternatives Considered

### Option A: Boolean `approved` flag on WorkTask
Simple, minimal. Store rejection reason as a nullable string on the task.
Rejected because it loses the original agent output once edited, cannot represent the
full Accept/EditAndAccept/Reject tri-state cleanly, and provides no hook for ButterflyService
propagation without adding special-case logic to the task entity.

### Option B: Event log (append-only gate events, no entity)
Record gate events as immutable domain events; derive current state by replaying.
Rejected because the current architecture does not use event sourcing. Querying gate state
(e.g. for the sprint board UI) would require replaying events on every read — unnecessary
complexity without full event sourcing infrastructure.

## References
- See `agent-design.md` for ButterflyService and workflow phase context
- See ADR-006 for TaskAttempt, which is the per-cycle complement to RevisionGate
