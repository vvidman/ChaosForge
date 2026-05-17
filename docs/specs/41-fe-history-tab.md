---
category: specs
title: "Task Attempt History Tab"
branch: "fe-history"
status: ready
date: "2026-04-21"
related_domain: [TaskAttempt, WorkTask]
related_adr: [006-task-attempt-per-cycle]
---

# Feature Spec — Task Attempt History Tab

<!-- Reference this file in the implementation agent with: implement @docs/specs/41-fe-history-tab.md -->

---

## Context

The history tab is the audit trail — every LLM output generated for every task is
accessible here. Users can review what the agents actually produced, read rejection
notes, and trace the evolution of a task through multiple attempts. This is a read-only
view. Depends on: specs 32–34, 36, 39 (linked from task card context menu).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Route: `/projects/:id/history` with optional query param `?task=:taskId` for
  pre-filtering (linked from sprint board task context menu).
- Layout: master-detail — left panel is a filterable WorkTask list; right panel shows
  the attempts for the selected task.
- **Left panel — task list:**
  - Search input filtering tasks by title (client-side, instant).
  - Status filter chips: All / Backlog / InProgress / InReview / InTesting /
    InDocumentation / Done.
  - Each task row: title, status badge, story points, attempt count.
  - Active task row highlighted with brand accent.
  - When `?task=:taskId` query param is present, pre-select that task on mount.
- **Right panel — attempt timeline:**
  - Header: task title, status badge, description (collapsed).
  - Timeline of `TaskAttempt` records sorted by `startedAt` ascending.
  - Each attempt is a card in the timeline:
    ```
    ┌──────────────────────────────────────────┐
    │ [AttemptType badge]   [AttemptResult]    │
    │ Agent: Persona-name   Started: 3m ago    │
    │                                          │
    │ ▶ Output (collapsed by default)          │
    │   [Expand ↓]                             │
    │                                          │
    │ ⚠ Rejection note: "..."  (if rejected)  │
    └──────────────────────────────────────────┘
    ```
  - Output expanded state: full markdown render (same `AgentOutputViewer` from spec 37).
  - `Pending` attempts show a spinning icon and "Agent is working..." placeholder.
  - Timeline connector line between attempts (vertical line with dot markers).
- **Empty state:** when no task is selected — "Select a task to view its attempt history".
- **No attempts:** when task has no attempts — "No attempts recorded yet".
- `TaskAttemptCompleted` and `TaskAttemptResolved` SignalR events trigger a refetch of
  the attempt list for the currently selected task, so new attempts appear in real time.

### AttemptType badge colors

- `Implementation` → brand blue
- `Review` → purple
- `Testing` → amber
- `Documentation` → teal

### AttemptResult badge colors

- `Pending` → gray with spinner
- `Approved` → green
- `Rejected` → red

---

## Implementation Scope — What must be done

- [ ] Create `src/pages/project/HistoryPage.tsx` with master-detail layout
- [ ] Create `src/components/history/TaskListPanel.tsx` with search + status filter
- [ ] Create `src/components/history/TaskListRow.tsx`
- [ ] Create `src/components/history/AttemptTimeline.tsx`
- [ ] Create `src/components/history/AttemptCard.tsx` with expandable output
- [ ] Create `src/components/history/AttemptTypeBadge.tsx`
- [ ] Create `src/components/history/AttemptResultBadge.tsx`
- [ ] Handle `?task=:taskId` query param for pre-selection
- [ ] Wire real-time refetch on `TaskAttemptCompleted` and `TaskAttemptResolved` events
  for the selected task (extend SignalR dispatch in spec 34)
- [ ] Wire route `/projects/:id/history` to `HistoryPage`
- [ ] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement attempt deletion
- Do not implement filtering by agent or date range — title + status filter is sufficient

---

## Test Expectations

- Unit tests required for:
  - `TaskListPanel` filters tasks correctly by status chip and search string
  - `AttemptCard` shows rejection note section only when `result === 'Rejected'`
  - `AttemptCard` shows spinner when `result === 'Pending'`
- Edge cases to cover: search returns no results — shows "No tasks match" empty state

---

## Open Questions

- None.
