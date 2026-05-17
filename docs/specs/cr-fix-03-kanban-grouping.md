---
category: specs
title: "CR Fix: KanbanBoard incorrect grouping logic"
branch: "fix-kanban-grouping"
status: ready
date: "2026-04-25"
related_domain: [WorkTask]
related_adr: []
---

# Feature Spec — CR Fix: KanbanBoard incorrect grouping logic

<!-- Reference this file in the implementation agent with: implement @docs/specs/cr-fix-03-kanban-grouping.md -->

---

## Context

`KanbanBoard.tsx` groups tasks using this logic:

```typescript
const col = task.sprintId === null ? 'Backlog' : task.status
```

This is incorrect. A task in `Backlog` status that has been assigned a `sprintId` will be
placed in the `Backlog` column by its status — which is correct. But a task that has
progressed to `InProgress` and then been rejected back to `Backlog` will have a non-null
`sprintId` and will be placed in its current `status` column — also correct.

The bug is that a task with `sprintId !== null` but `status === 'Backlog'` (i.e. a task
waiting to start its sprint) will be placed in the `Backlog` column by the `else` branch...
actually wait, the `else` branch places it in `task.status` which IS `Backlog`. That is
correct.

The real bug: any task with `sprintId === null` is forced into `Backlog` regardless of its
actual `status`. This should never happen in normal flow (tasks get sprintId before starting),
but pure backlog tasks (not yet sprint-assigned) do have `sprintId === null` and `status ===
'Backlog'`. The current logic correctly places those in `Backlog`.

However: the special case `sprintId === null → Backlog` overrides `task.status`, meaning
if a task somehow had `sprintId === null` and `status !== 'Backlog'` (a data integrity
issue), it would be silently misplaced. More importantly, it adds cognitive overhead and
hides the actual intent.

The correct, simple implementation is: **always group by `task.status`**. The `sprintId`
condition adds no real value for display purposes. Tasks in `Backlog` status are shown in
the Backlog column. Done.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Remove the `sprintId` null check. Group solely by `task.status`.
- This is a one-line change in `KanbanBoard.tsx`.

---

## Implementation Scope — What must be done

- [ ] Update `src/components/sprint/KanbanBoard.tsx`:
  ```typescript
  // Remove:
  const col = task.sprintId === null ? 'Backlog' : task.status

  // Replace with:
  const col = task.status
  ```

- [ ] Run `npm run build` — zero TypeScript errors
- [ ] Update the existing `KanbanBoard` unit test to reflect the corrected grouping logic
  (test that tasks are grouped by `status`, not by `sprintId`)

---

## Out of Scope — What must NOT be done

- Do not change column ordering or column definitions

---

## Test Expectations

- Unit tests required for:
  - `KanbanBoard` groups a task with `sprintId: null` and `status: 'Backlog'` into
    the `Backlog` column (unchanged behaviour)
  - `KanbanBoard` groups a task with `sprintId: 'some-guid'` and `status: 'InReview'`
    into the `InReview` column (was previously also correct, now explicitly tested)

---

## Open Questions

- None.
