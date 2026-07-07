---
category: specs
title: "Revision Gate Judge Interface"
branch: "fe-gate-judge"
status: done
date: "2026-04-21"
related_domain: [RevisionGate]
related_adr: [005-revision-gate-entity]
---

# Feature Spec — Revision Gate Judge Interface

<!-- Reference this file in the implementation agent with: implement @docs/specs/37-fe-gate-judge.md -->

---

## Context

The revision gate is the most important human touchpoint in ChaosForge. When an agent
completes a phase-level task, the human must Accept, EditAndAccept, or Reject before
the project advances. The UI must make the decision prominent, clear, and hard to
accidentally trigger. This is a dedicated full-screen page, not a dialog. Depends on:
specs 32–34, 36.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Route: `/projects/:id/gate` — accessed from the `OpenGateBanner` in spec 36.
- This route renders a full-screen layout (no sidebar, no tabs) to keep focus on the
  review. The top bar shows the project name and a "← Back to project" link.
- If no open gate exists for the project: redirect to `/projects/:id/overview`.
- Layout: two-column (60/40 split on desktop, stacked on mobile):
  - Left column: agent output in a styled markdown renderer
    (`react-markdown` with `remark-gfm` for tables/code blocks)
  - Right column: decision panel
- Decision panel sections (top to bottom):
  1. Gate type header: "Requirements Review", "Architecture Review",
     or "Sprint Planning Review"
  2. Status: "Open — awaiting your decision"
  3. Three action buttons (full-width, stacked):
     - **Accept** (green): opens a confirmation dialog
     - **Edit & Accept** (brand blue): expands an inline editor
     - **Reject** (red): expands a rejection reason textarea
  4. Expanded state for Edit & Accept:
     - Full-height textarea pre-filled with agent output
     - "Submit edit" button (calls `editAndAcceptGate`)
  5. Expanded state for Reject:
     - Textarea labeled "Rejection reason (required)"
     - "Submit rejection" button (disabled until textarea has content)
  6. Only one expanded state visible at a time — selecting a second collapses the first
- Confirmation dialog for Accept: "This will advance the project to the next phase.
  Continue?" with Cancel / Confirm buttons.
- After any successful decision: navigate to `/projects/:id/overview` and show a
  success toast.
- Loading state: full-screen skeleton while gate data is fetching.
- Error state: if gate fetch fails, show an error card with a retry button.
- Markdown renderer styling: code blocks with syntax highlight (use `react-syntax-highlighter`
  with the `oneDark` theme), headers styled with `@tailwindcss/typography` prose class.

### Agent output presentation

- Monospaced font in code blocks
- Max height 70vh with scroll — the output can be very long
- Copy-to-clipboard button in top-right of the output panel

---

## Implementation Scope — What must be done

- [x] Install `react-markdown`, `remark-gfm`, `react-syntax-highlighter`,
  `@types/react-syntax-highlighter`
- [x] Create `src/pages/project/GatePage.tsx` with full-screen layout
- [x] Create `src/components/gate/AgentOutputViewer.tsx`:
  - Renders markdown with syntax highlighting
  - Copy-to-clipboard button
  - Scrollable container
- [x] Create `src/components/gate/DecisionPanel.tsx`:
  - Three action buttons with expand/collapse logic
  - Edit textarea (pre-filled from `agentOutput`)
  - Rejection reason textarea with character counter
  - Confirmation dialog for Accept
- [x] Create `src/components/gate/GateTypeHeader.tsx` — maps `RevisionGateType` to
  a human-readable title and description of what the agent produced
- [x] Wire mutations: `acceptGate`, `editAndAcceptGate`, `rejectGate`
- [x] Add route `/projects/:id/gate` to router (full-screen layout, no sidebar)
- [x] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not show the resolution history of prior rejected gates on this page
- Do not implement gate creation from the frontend — the backend opens gates automatically

---

## Test Expectations

- Unit tests required for:
  - `DecisionPanel`: only one expanded state visible at a time (collapse others on open)
  - Reject submit button disabled when textarea is empty
- Edge cases to cover: `editAndAcceptGate` called with empty string is prevented
  client-side (textarea must have content)

---

## Open Questions

- None.
