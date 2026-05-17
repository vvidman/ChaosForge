---
category: specs
title: "Project Detail Shell and Overview Tab"
branch: "fe-proj-detail"
status: done
date: "2026-04-21"
related_domain: [Project, AgentSlot, RevisionGate]
related_adr: []
---

# Feature Spec — Project Detail Shell and Overview Tab

<!-- Reference this file in the implementation agent with: implement @docs/specs/36-fe-project-detail.md -->

---

## Context

Every project-level feature (requirements, sprint, agents, history) lives under a shared
project detail shell. This spec implements that shell — tab navigation, project header,
lifecycle phase stepper — and fills in the Overview tab. The other tabs are placeholders
until their respective specs. Depends on: specs 32–33, 35.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Route: `/projects/:id` with nested routes for each tab. The shell is the layout
  component for these nested routes.
- Tab routes: `/overview` (default redirect), `/requirements`, `/sprint`, `/agents`,
  `/history`.
- Shell header: project name (h1), current status badge, description (editable inline
  — see below), deadline.
- **Inline description edit:** clicking the description renders a textarea + Save/Cancel.
  Save calls `updateProjectDescription` mutation. On success: show toast, exit edit mode.
- **Phase stepper:** horizontal stepper showing all 6 phases in order. Current phase is
  highlighted with brand color. Completed phases have a checkmark. Future phases are muted.
  Clicking a step does nothing — it is display only.
- **Phase action button:** below the stepper, context-sensitive:
  - `Setup` → "Start Requirements Phase" button → calls `transitionProject`
  - `RequirementsPhase` / `ArchitecturePhase` / `SprintPlanning` → shows
    "Waiting for agent..." spinner if no open gate; shows "Review gate →" link if gate open
  - `Development` → no button (agents work autonomously)
  - `Completed` → "Project complete" banner
- **Open gate banner:** when `getOpenRevisionGate` returns a gate, show a prominent
  amber banner at the top of the shell: "Human review required — [gate type]" with a
  "Review now →" button that navigates to `/projects/:id/gate`.
- Overview tab content: project metadata card + AgentSlot configuration table.
  AgentSlot table shows role, count, and an edit count button (inline spinner input).
  Only editable during `Setup` phase.

### AgentSlot table anatomy

```
Role              Count   (edit)
BusinessAnalyst   1       [−] 1 [+]   (singleton — buttons disabled)
Architect         1       [−] 1 [+]   (singleton)
ScrumMaster       1       [−] 1 [+]   (singleton)
Developer         2       [−] 2 [+]
Tester            1       [−] 1 [+]
Reviewer          1       [−] 1 [+]
TechnicalWriter   1       [−] 1 [+]
```

On project create, one AgentSlot per role is automatically shown with count=1 and a
"Configure" action. Slot creation goes through `createAgentSlot` mutation.

---

## Implementation Scope — What must be done

- [x] Create `src/layouts/ProjectDetailLayout.tsx` with tab nav and project header
- [x] Create `src/components/projects/PhaseStepper.tsx`
- [x] Create `src/components/projects/OpenGateBanner.tsx`
- [x] Create `src/components/projects/InlineEditDescription.tsx`
- [x] Create `src/components/projects/AgentSlotTable.tsx` with +/− count controls
- [x] Create `src/pages/project/OverviewPage.tsx`
- [x] Create placeholder page components for `/requirements`, `/sprint`, `/agents`,
  `/history` (each renders a "Coming soon" heading — replaced in later specs)
- [x] Wire nested routes under `/projects/:id`
- [x] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement the gate review UI here — that is spec 37
- Do not implement the requirements, sprint, agent, history tabs — later specs

---

## Test Expectations

- Unit tests required for: `PhaseStepper` renders correct active/completed/future state
  for each `ProjectStatus` value
- Edge cases to cover: `Completed` status shows all steps as completed

---

## Open Questions

- None.
