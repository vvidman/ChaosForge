---
category: specs
title: "Sprint Board Tab"
branch: "fe-sprint-board"
status: ready
date: "2026-04-21"
related_domain: [WorkTask, TaskAttempt]
related_adr: [006-task-attempt-per-cycle]
---

# Feature Spec — Sprint Board Tab

<!-- Reference this file in the implementation agent with: implement @docs/specs/39-fe-sprint-board.md -->

---

## Context

The sprint board gives the user a real-time kanban view of all WorkTasks and their
progress through the development pipeline. Columns update live via SignalR. Users can
also manually reject tasks (returning them to Backlog) when needed. This is the most
data-dense and visually active view in the application. Depends on: specs 32–34, 36.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Route: `/projects/:id/sprint`
- Layout: horizontal scrolling kanban with 6 columns (one per `WorkTaskStatus`).
  On mobile: single visible column with prev/next navigation arrows.
- Columns: `Backlog`, `InProgress`, `InReview`, `InTesting`, `InDocumentation`, `Done`.
- Column header: status name + task count badge.
- Task card:
  ```
  ┌─────────────────────────────┐
  │ [Story points badge]        │
  │                             │
  │ Task title (semibold)       │
  │ Description (2 lines max)   │
  │                             │
  │ [Latest attempt type badge] │
  │                [⋮ menu]     │
  └─────────────────────────────┘
  ```
- Task card context menu (⋮): opens a `Tooltip`-anchored dropdown:
  - "View attempts →" — navigates to `/projects/:id/history?task=:taskId`
  - "Reject task" — only shown when task is `InReview` or `InTesting`; opens a
    confirmation dialog with a reason field; calls `rejectTask` mutation.
- **Real-time updates:** when `WorkTaskStatusChanged` event fires (spec 34), the task
  card animates from one column to another. Use a CSS `transition` on `transform` for
  a slide effect. Implementation: move the card by re-rendering into the new column;
  use a `layout` key trick so React animates the change.
- **Animated counter:** column task count badges animate when count changes (brief
  scale-up + color flash using Tailwind `animate-pulse` for 800ms).
- **Empty column:** shows a muted "No tasks" placeholder, not an error.
- **Board header:** sprint ID (displayed as first 8 chars of Guid), total story points
  in sprint, completed story points.
- Tasks are fetched using `getWorkTasksByProject` — uses the new `GetByProjectIdAsync`
  endpoint added in the bug fix, grouped client-side by `status`.
- WorkTasks without `sprintId` (pure Backlog) are shown in the Backlog column.

### Reject flow

1. User clicks "Reject task" in context menu
2. Dialog opens: "Reason for rejection" textarea (required, min 10 chars)
3. Confirm button calls `rejectTask(taskId)` — NOTE: the backend `RejectWorkTaskCommand`
   does not accept a reason; the reason is for the human's reference only (logged but
   not persisted in this flow — the reject transitions the task back to Backlog)
4. On success: toast + card animates back to Backlog column

---

## Implementation Scope — What must be done

- [ ] Create `src/pages/project/SprintPage.tsx`
- [ ] Create `src/components/sprint/KanbanBoard.tsx` — column layout
- [ ] Create `src/components/sprint/KanbanColumn.tsx` — header + card list
- [ ] Create `src/components/sprint/TaskCard.tsx` — card with context menu
- [ ] Create `src/components/sprint/RejectTaskDialog.tsx`
- [ ] Create `src/components/sprint/BoardHeader.tsx` — sprint stats
- [ ] Implement column animation on status change (CSS transition)
- [ ] Wire route `/projects/:id/sprint` to `SprintPage`
- [ ] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement drag-to-move between columns — task status is controlled by agents
- Do not implement manual task creation from the board — tasks are created by the Architect
- Do not implement sprint selection — there is only one sprint per project at this stage

---

## Test Expectations

- Unit tests required for:
  - `KanbanBoard` groups tasks correctly by status
  - `TaskCard` shows "Reject task" menu item only for `InReview` and `InTesting`
- Edge cases to cover: task with no attempts shows no attempt badge, not an error

---

## Open Questions

- None.
