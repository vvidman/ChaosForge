---
category: specs
title: "Project List and Create Page"
branch: "fe-proj-list"
status: done
date: "2026-04-21"
related_domain: [Project]
related_adr: []
---

# Feature Spec — Project List and Create Page

<!-- Reference this file in the implementation agent with: implement @docs/specs/35-fe-project-list.md -->

---

## Context

The entry point of the application. Users see all projects, create new ones, and navigate
into a project. This is the first real UI after the shell. Depends on: specs 32–33.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Page component: `src/pages/ProjectListPage.tsx`
- Empty state: illustrated empty state card with "Create your first project" CTA.
- Project card: shows name, description (truncated to 2 lines), status badge,
  deadline (if set), `createdAt` relative time ("3 days ago").
- Status badge colors map to Tailwind `status.*` design tokens from spec 32.
- Create project: opens a shadcn `Dialog` (not a new route). Form fields:
  Name (required), Description (required), Deadline (optional date picker).
- Form validation: client-side with React Hook Form + Zod (`react-hook-form`, `zod`).
  Inline error messages below each field.
- On successful create: close dialog, show success toast, React Query invalidates
  `['projects']` automatically via the mutation hook.
- On API error: show error message inside the dialog (do not close it).
- Loading state: project cards replaced by `Skeleton` components while loading.
- The list is sorted by `createdAt` descending (newest first).
- Clicking a card navigates to `/projects/:id/overview`.

### Project card anatomy

```
┌──────────────────────────────────────┐
│ [Status badge]          [Deadline]   │
│                                      │
│ Project name (semibold, lg)          │
│ Description line 1                   │
│ Description line 2...                │
│                                      │
│ Created 3 days ago        →          │
└──────────────────────────────────────┘
```

---

## Implementation Scope — What must be done

- [x] Install `react-hook-form` and `zod`
- [x] Create `src/components/projects/ProjectCard.tsx`
- [x] Create `src/components/projects/CreateProjectDialog.tsx` with form + validation
- [x] Create `src/components/ui/StatusBadge.tsx` — maps any status string to a colored
  badge using design tokens; used across the whole app
- [x] Create `src/components/ui/RelativeTime.tsx` — renders "X ago" from ISO string
- [x] Create `src/pages/ProjectListPage.tsx`:
  - Header with "Projects" title and "+ New Project" button
  - `Skeleton` grid while loading
  - `ProjectCard` grid (responsive: 1 col mobile, 2 col tablet, 3 col desktop)
  - Empty state illustration when `projects.length === 0`
  - `CreateProjectDialog` triggered by button
- [x] Wire route `/projects` to `ProjectListPage`
- [x] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement project deletion — not in scope for this project
- Do not implement project editing — only description can be updated (in detail view)
- Do not implement pagination — show all projects

---

## Test Expectations

- Unit tests required for: `StatusBadge` renders correct color class for each status;
  `RelativeTime` renders "just now" for timestamps < 1 minute ago
- Edge cases to cover: empty project list shows empty state, not an error

---

## Open Questions

- None.
