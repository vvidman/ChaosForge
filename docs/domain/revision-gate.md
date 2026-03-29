---
category: domain
area: "Revision Gate"
last_updated: "2026-03-29"
related_adr: [ADR-005]
---

# Revision Gate

## Overview
A `RevisionGate` is a mandatory human checkpoint between agent-driven phases. After the
BusinessAnalyst, Architect, and ScrumMaster complete their outputs, the human must explicitly
decide: Accept, EditAndAccept, or Reject. The gate is the only mechanism by which a project
advances to the next phase. Rejection blocks the project until the agent retries and a new
gate is resolved.

## Key Concepts

### RevisionGate

| Field | Type | Notes |
|---|---|---|
| Id | Guid | |
| ProjectId | Guid | |
| Type | RevisionGateType | AfterBA / AfterArchitect / AfterScrumMaster |
| Status | RevisionGateStatus | Open / Resolved |
| AgentOutput | string | original, unmodified agent output |
| HumanEditedOutput | string? | set only on EditAndAccept |
| RejectionReason | string? | set only on Reject; required |
| Action | RevisionGateAction? | null while Open |
| CreatedAt | DateTime | when the agent completed its output |
| ResolvedAt | DateTime? | when the human made a decision |

### RevisionGateType
| Value | Opened after | Unlocks |
|---|---|---|
| AfterBA | BusinessAnalyst completes URS | ArchitecturePhase |
| AfterArchitect | Architect completes SRS + WorkTasks | SprintPlanning |
| AfterScrumMaster | ScrumMaster completes sprint plan | Development |

### RevisionGateAction
- **Accept** — output is used as-is; project advances
- **EditAndAccept** — human modifies output; `HumanEditedOutput` is saved; ButterflyService
  propagates changes downstream; project advances
- **Reject** — agent must retry; `RejectionReason` is injected into the next attempt prompt;
  project stays in current phase

### ButterflyService
Triggered exclusively by `EditAndAccept`. Propagates the human's edits downstream:
- AfterBA edit → Architect's SRS generation prompt is updated
- AfterArchitect edit → existing WorkTasks derived from modified SRS are flagged for regeneration
- AfterScrumMaster edit → sprint assignment may be revised

ButterflyService is a domain service — it does not call infrastructure directly.

### Gate lifecycle
```
[Agent completes output]
  → RevisionGate created, Status = Open
  → Human decides
      Accept          → Status = Resolved, project phase advances
      EditAndAccept   → HumanEditedOutput set, ButterflyService fires, Status = Resolved, phase advances
      Reject          → RejectionReason set, Status = Resolved, agent retries, new gate will open
```

## Business Rules
- Exactly one `RevisionGate` of each type exists per project (one AfterBA, one AfterArchitect, one AfterScrumMaster); on retry, the previous gate is Resolved (Rejected) and a new gate is opened
- `RejectionReason` is mandatory when `Action = Reject` — empty string is not accepted
- `HumanEditedOutput` is mandatory when `Action = EditAndAccept`
- A project phase may not advance while any `RevisionGate` of the corresponding type has `Status = Open`
- `AgentOutput` is immutable after the gate is created — human edits go to `HumanEditedOutput` only

## Boundaries
**In scope:** RevisionGate entity; Accept/EditAndAccept/Reject decision logic; ButterflyService trigger rules; gate lifecycle
**Out of scope:** what the agent output contains (see `requirements-pipeline.md`), sprint contents (see `work-execution.md`), phase transition rules (see `project-lifecycle.md`)

## Related Areas
- `project-lifecycle.md` — phase transitions that gates unlock
- `requirements-pipeline.md` — URS and SRS that ButterflyService may modify
- `work-execution.md` — WorkTasks that may be invalidated by ButterflyService

## References
- See ADR-005 for the rationale behind RevisionGate as a first-class entity
- See `agent-design.md` (architecture) for ButterflyService in execution context
