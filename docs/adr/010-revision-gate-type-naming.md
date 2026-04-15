---
category: adr
id: "010"
title: "RevisionGateType naming: phase-centric over agent-centric"
status: accepted
date: "2026-04-12"
supersedes: null
superseded_by: null
related_principles: [agent-design, clean-architecture]
---

# ADR-010: RevisionGateType naming: phase-centric over agent-centric

## Status
`accepted`

## Context

The planning specifications (specs 07, 22–26) referred to revision gate types as
`AfterBA`, `AfterArchitect`, and `AfterScrumMaster` — names derived from the agent role
that precedes each gate. During implementation the enum values were renamed to
`Requirements`, `Architecture`, and `SprintPlanning`.

This discrepancy was identified during a code review (spec 21–31 implementation cycle)
and documented here rather than reverting the implementation, because the implementation
choice is the better design.

## Decision

`RevisionGateType` enum values use phase-centric names:

| Spec name | Implemented name | Phase guarded |
|---|---|---|
| `AfterBA` | `Requirements` | RequirementsPhase output |
| `AfterArchitect` | `Architecture` | ArchitecturePhase output |
| `AfterScrumMaster` | `SprintPlanning` | SprintPlanning output |

The implementation names are canonical. The spec names are considered aliases and are not
used in code. All future specs, ADRs, and domain documentation must use the implemented
names (`Requirements`, `Architecture`, `SprintPlanning`).

## Consequences

**Positive:**
- Phase-centric names couple the gate to *what it guards*, not *who produced it*. If an
  agent role were renamed or replaced, the gate type name would remain stable.
- Consistent with `ProjectStatus` naming (`RequirementsPhase`, `ArchitecturePhase`,
  `SprintPlanning`) — the gate type and the phase it closes share the same conceptual vocabulary.
- Reduces cognitive load when reasoning about the workflow: "the Requirements gate" maps
  directly to "the RequirementsPhase", without needing to know which agent role runs in it.

**Trade-offs:**
- The planning specs use agent-centric names. Readers cross-referencing specs against code
  must be aware of this mapping. This ADR serves as the authoritative reference.

## Alternatives Considered

### Option A: Revert to agent-centric names (`AfterBA`, `AfterArchitect`, `AfterScrumMaster`)
Aligns with spec wording. Rejected because it couples the enum to agent role names, which
are an implementation detail of the current agent design. A future redesign that renames or
merges roles would require renaming the enum — a wider change than necessary.

### Option B: Use both (aliases or separate types)
Add `[Obsolete]`-tagged aliases or a mapping layer between spec names and code names.
Rejected because it adds noise for a hobby project; a single ADR is sufficient to document
the discrepancy without adding runtime complexity.

## References
- See ADR-005 for the rationale behind RevisionGate as a first-class entity
- See `revision-gate.md` (domain) for gate lifecycle — note that doc uses `AfterBA`-style
  names inherited from early planning; treat this ADR as the naming override
- Specs 07, 22–26 use agent-centric names and should be read with this mapping in mind
