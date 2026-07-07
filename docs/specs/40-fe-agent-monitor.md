---
category: specs
title: "Agent Monitor Tab"
branch: "fe-agent-mon"
status: done
date: "2026-04-21"
related_domain: [AgentInstance, AgentSlot]
related_adr: [003-background-service-workers]
---

# Feature Spec — Agent Monitor Tab

<!-- Reference this file in the implementation agent with: implement @docs/specs/40-fe-agent-monitor.md -->

---

## Context

The agent monitor is the "mission control" view — it shows every agent instance, their
current status, and which task they are working on in real time. Status changes are
pushed via SignalR. This is a read-heavy, update-frequently view with no user mutations.
Depends on: specs 32–34, 36.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Route: `/projects/:id/agents`
- Layout: agent cards in a responsive grid (2 cols desktop, 1 col mobile), grouped by
  role category: Singleton Agents (BA, Architect, SM), Development Agents (rest).
- Agent card:
  ```
  ┌────────────────────────────────┐
  │ [Role icon] BusinessAnalyst    │
  │             ● Working          │  ← animated status dot
  │                                │
  │ Persona: BA-3f8a1b2c           │
  │ Task: [task title or "—"]      │
  │                                │
  │ [View task →]                  │  ← only if currentTaskId set
  └────────────────────────────────┘
  ```
- Status dot animations:
  - `Idle` — gray, static
  - `Working` — brand blue, pulsing (`animate-pulse`)
  - `Blocked` — red, static
  - `Finished` — green, static
- **Real-time status update:** when `AgentStatusChanged` SignalR event fires, the status
  dot and label update without a full card re-render. The transition is animated with a
  CSS `transition` on `color` and `background-color`.
- "View task →" link navigates to `/projects/:id/history?task=:taskId` (history tab
  pre-filtered to that task).
- Role icons: Lucide icons mapped per role:
  - BusinessAnalyst → `Brain`
  - Architect → `Building`
  - ScrumMaster → `ClipboardList`
  - Developer → `Code2`
  - Tester → `FlaskConical`
  - Reviewer → `Search`
  - TechnicalWriter → `FileText`
- **Live event log:** below the agent grid, a scrollable "Recent events" feed showing
  the last 50 `AgentStatusChanged` and `WorkTaskStatusChanged` events received via
  SignalR since the page loaded. Each entry: timestamp, icon, description
  (e.g. "Developer BA-3f8a1b2c → Working on 'Implement login endpoint'").
  This is purely in-memory — it is cleared on navigation away.
- **Phase indicator banner:** shows which phase is currently active and which roles
  should be running. If a role expected in the current phase has no `Idle` or `Working`
  instances, show a warning icon next to that role header.

---

## Implementation Scope — What must be done

- [x] Create `src/pages/project/AgentsPage.tsx`
- [x] Create `src/components/agents/AgentCard.tsx` with animated status dot
- [x] Create `src/components/agents/AgentGrid.tsx` with role grouping
- [x] Create `src/components/agents/EventLog.tsx` — in-memory scrolling event feed,
  updated via a Zustand slice that SignalR handler pushes to
- [x] Create `src/components/agents/PhaseIndicatorBanner.tsx`
- [x] Add `AgentStatusChanged` events to the in-memory EventLog Zustand slice
  (extend spec 34's Zustand store)
- [x] Wire route `/projects/:id/agents` to `AgentsPage`
- [x] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement agent instance creation from this tab — done via Overview tab
- Do not implement agent blocking/unblocking controls — agents manage their own state

---

## Test Expectations

- Unit tests required for:
  - `AgentCard` renders correct icon per role
  - `AgentCard` shows "View task →" link only when `currentTaskId` is set
  - Phase indicator shows warning when expected role has no active instances
- Edge cases to cover: all agents `Finished` — no warning shown (project may be complete)

---

## Open Questions

- None.
