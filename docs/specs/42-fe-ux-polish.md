---
category: specs
title: "Global UX Polish — Loading, Errors, Animations, Accessibility"
branch: "fe-ux-polish"
status: ready
date: "2026-04-21"
related_domain: []
related_adr: []
---

# Feature Spec — Global UX Polish

<!-- Reference this file in the implementation agent with: implement @docs/specs/42-fe-ux-polish.md -->

---

## Context

Specs 35–41 implement functional pages with basic loading and error states. This spec
upgrades them to polished, production-quality UX: consistent skeleton loading, error
boundaries with recovery, page transitions, micro-animations, keyboard navigation, and
accessibility (ARIA). Depends on: all previous frontend specs.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- **No new third-party animation library.** All animations use Tailwind utilities
  (`transition`, `duration-*`, `ease-*`) and CSS custom properties. Framer Motion and
  React Spring are explicitly excluded — overkill for this scope.
- Error boundaries implemented with React's `ErrorBoundary` class component pattern,
  wrapped around each page-level route. The boundary renders a fallback UI — not a
  blank screen.
- All interactive elements (buttons, links, inputs) must have visible focus rings
  using Tailwind's `focus-visible:ring-2` pattern.
- Keyboard navigation: the sidebar must be navigable with Tab and arrow keys.
  The kanban board columns must be Tab-focusable.

### Skeleton loading

Replace all ad-hoc loading states with a consistent `<Skeleton>` system:
- `SkeletonCard` — matches the project card dimensions
- `SkeletonTable` — for agent slot table
- `SkeletonTimeline` — for attempt history
- `SkeletonKanbanColumn` — for sprint board column
All skeletons use `animate-pulse` with `surface.border` color.

### Error boundary UI

```
┌────────────────────────────────────────┐
│  ⚠  Something went wrong               │
│                                         │
│  [Error message]                        │
│                                         │
│  [Try again]    [Go to projects]        │
└────────────────────────────────────────┘
```

"Try again" resets the boundary state. "Go to projects" navigates to `/projects`.

### Page transitions

Wrap `<Outlet />` in `AppLayout` with a fade-in transition on route change:
- `opacity: 0 → 1` over 150ms using `transition-opacity`
- Triggered by React Router's `useLocation` key change
- Implemented via a wrapper component `<PageTransition>` that toggles a CSS class

### Micro-animations checklist

- [ ] Status badge color change: `transition-colors duration-300`
- [ ] Task card move between kanban columns: CSS transition on transform (spec 39
  already mentions this — formalize here)
- [ ] Agent status dot pulse: `animate-pulse` on Working, static on others
- [ ] Toast slide-in: `translate-y-full → translate-y-0` over 200ms
- [ ] Toast slide-out: reverse on dismiss
- [ ] Sidebar collapse: `width` transition 240px → 48px over 200ms
- [ ] Dialog open: `scale(0.95) opacity(0) → scale(1) opacity(1)` (shadcn provides
  this via Radix — verify it's enabled)
- [ ] Accordion open (SRS expand, output expand): `max-height` transition

### Accessibility

- All icon-only buttons have `aria-label`
- Status badges have `role="status"` and `aria-label` with the full text
- Loading skeletons have `aria-busy="true"` and `aria-label="Loading..."`
- Error boundary has `role="alert"`
- Form fields have associated `<label>` elements (not just placeholders)
- Color is never the only indicator of state (badges also have text)
- `prefers-reduced-motion`: wrap all `animate-pulse` and transitions in a
  `@media (prefers-reduced-motion: reduce)` check — disable animations if set

---

## Implementation Scope — What must be done

- [ ] Create `src/components/ui/SkeletonCard.tsx`
- [ ] Create `src/components/ui/SkeletonTable.tsx`
- [ ] Create `src/components/ui/SkeletonTimeline.tsx`
- [ ] Create `src/components/ui/SkeletonKanbanColumn.tsx`
- [ ] Create `src/components/error/ErrorBoundary.tsx` (class component)
- [ ] Create `src/components/error/ErrorFallback.tsx` (fallback UI)
- [ ] Wrap each page route in `ErrorBoundary` in `src/router.tsx`
- [ ] Create `src/components/layout/PageTransition.tsx`
- [ ] Wrap `<Outlet />` in `AppLayout` with `PageTransition`
- [ ] Implement sidebar collapse toggle (button in sidebar footer, state in Zustand)
- [ ] Apply `transition-colors` to `StatusBadge`
- [ ] Apply `focus-visible:ring-2` to all interactive elements
- [ ] Add `aria-label` to all icon-only buttons project-wide
- [ ] Add `aria-busy` + `aria-label` to all skeleton components
- [ ] Add `prefers-reduced-motion` guard to Tailwind config:
  ```javascript
  // tailwind.config.ts
  plugins: [
    plugin(({ addBase }) => {
      addBase({
        '@media (prefers-reduced-motion: reduce)': {
          '*': { 'animation-duration': '0.01ms !important',
                 'transition-duration': '0.01ms !important' },
        },
      })
    }),
  ]
  ```
- [ ] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not add Framer Motion or any animation library
- Do not implement dark/light mode toggle — the app is dark-only
- Do not implement internationalization

---

## Test Expectations

- Unit tests required for:
  - `ErrorBoundary` renders fallback when child throws
  - `ErrorBoundary` resets on "Try again" click
- Edge cases to cover: nested `ErrorBoundary` — inner boundary catches, outer does not

---

## Open Questions

- None.
