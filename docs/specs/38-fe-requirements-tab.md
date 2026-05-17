---
category: specs
title: "Requirements Pipeline Tab"
branch: "fe-req-tab"
status: ready
date: "2026-04-21"
related_domain: [UseCase, URS, SRS]
related_adr: []
---

# Feature Spec — Requirements Pipeline Tab

<!-- Reference this file in the implementation agent with: implement @docs/specs/38-fe-requirements-tab.md -->

---

## Context

The requirements pipeline tab gives the user a read-only view of the full UseCase → URS → SRS
hierarchy and the ability to create UseCases during the Setup phase. It also provides human
edit affordances for URS and SRS items when the project is in the appropriate phase.
Depends on: specs 32–34, 36.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Route: `/projects/:id/requirements`
- Layout: three-column accordion hierarchy — UseCase → URS → SRS, each expandable.
  On mobile: single column with breadcrumb navigation.
- **UseCase panel** (always visible):
  - List of all UseCases for the project, sorted by `priority` ascending.
  - Each UseCase: title, description (truncated), priority badge.
  - During `Setup` phase: "+ Add Use Case" button at the bottom opens an inline form
    (not a dialog) — fields: Title, Description, Priority (number input, default 0).
  - Priority can be reordered during Setup via `updateUseCasePriority` mutation
    (drag-to-reorder using `@dnd-kit/core` and `@dnd-kit/sortable`).
- **URS panel** (shown when a UseCase is selected):
  - List of URS items linked to the selected UseCase.
  - Shown as cards with title and description.
  - If project is in `RequirementsPhase` or later and URS items exist:
    show "Human edit" button per URS item. Opens an inline editor (textarea pre-filled
    with current description + note field). Save calls `applyHumanEditToURS`.
  - If no URS items yet: show "Waiting for Business Analyst..." placeholder with a
    spinning icon (only shown in `RequirementsPhase`).
- **SRS panel** (shown when a URS is selected):
  - List of SRS items linked to the selected URS.
  - Each SRS: title, technical description (collapsed to 3 lines, expandable).
  - "Human edit" affordance same pattern as URS — only in `ArchitecturePhase` or later.
  - WorkTasks derived from this SRS shown as a compact list (title + status badge only).
- **Skeleton** loading state for each panel while fetching.
- Column highlight: selected UseCase and URS have a left accent border in brand color.

### Drag-to-reorder priority

- `@dnd-kit/core` + `@dnd-kit/sortable` for UseCase priority reorder.
- On drop: call `updateUseCasePriority` for each item whose index changed.
- Only active during `Setup` phase — drag handles hidden in other phases.

---

## Implementation Scope — What must be done

- [ ] Install `@dnd-kit/core` and `@dnd-kit/sortable`
- [ ] Create `src/pages/project/RequirementsPage.tsx`
- [ ] Create `src/components/requirements/UseCaseList.tsx` with add form and drag-to-sort
- [ ] Create `src/components/requirements/UseCaseCard.tsx`
- [ ] Create `src/components/requirements/URSPanel.tsx` with human edit inline editor
- [ ] Create `src/components/requirements/SRSPanel.tsx` with human edit + WorkTask list
- [ ] Create `src/components/requirements/InlineEditor.tsx` — reusable textarea + note
  + Save/Cancel for both URS and SRS edit flows
- [ ] Create `src/components/requirements/WaitingForAgent.tsx` — spinner + label
- [ ] Wire route `/projects/:id/requirements` to `RequirementsPage`
- [ ] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement UseCase deletion
- Do not implement SRS creation — that is done by the Architect agent
- Do not show full WorkTask details here — that is the sprint board tab

---

## Test Expectations

- Unit tests required for:
  - `UseCaseList` renders add form only when `projectStatus === 'Setup'`
  - `InlineEditor` disables Save button when both fields are empty
- Edge cases to cover: URS human edit note field is optional — Save must work without it

---

## Open Questions

- None.
