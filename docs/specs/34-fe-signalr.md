---
category: specs
title: "SignalR Client and Real-Time State"
branch: "fe-signalr"
status: done
date: "2026-04-21"
related_domain: []
related_adr: [009-signalr-events]
---

# Feature Spec — SignalR Client and Real-Time State

<!-- Reference this file in the implementation agent with: implement @docs/specs/34-fe-signalr.md -->

---

## Context

The backend broadcasts domain events over SignalR whenever agent state changes, tasks
advance, or revision gates are resolved. Without a real-time client, the frontend only
shows stale data until manual refresh. This spec wires the SignalR connection, handles
reconnection, dispatches incoming events to React Query's cache and a Zustand
notification store. Depends on: spec 32 (Zustand, queryClient), spec 33 (query keys).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- SignalR connection is managed in a single React context (`SignalRContext`) provided at
  the root. Components never import `@microsoft/signalr` directly — they use the context
  hook `useSignalR()`.
- Connection lifecycle: connect on mount, reconnect automatically on disconnect
  (use `withAutomaticReconnect()`). Log connect/disconnect to console in dev.
- Connection status is stored in Zustand: `'connecting' | 'connected' | 'disconnected'`.
  The top bar connection dot reads from this store.
- Incoming message format from backend:
  ```json
  { "type": "EventName", "payload": { ... } }
  ```
  The client listens on the `"ReceiveEvent"` method and dispatches by `type`.
- Event dispatch strategy: **cache invalidation, not cache update**. When an event
  arrives, call `queryClient.invalidateQueries` for the affected query keys. This keeps
  the logic simple and correct — a refetch always gets fresh data.
- Exception: `AgentStatusChanged` and `WorkTaskStatusChanged` also push a toast
  notification to the Zustand notification queue so the user sees a brief status change
  message without having to watch the monitor screen.
- Toast notifications: non-blocking, auto-dismiss after 4 seconds, max 5 visible at once,
  stacked in bottom-right corner. Implemented as a Zustand slice + `ToastContainer`
  component. **Do not use a third-party toast library** — build it with Tailwind.

### Event → cache invalidation mapping

| Event type | Invalidates query keys |
|---|---|
| `ProjectStatusChanged` | `['projects']`, `['project', projectId]` |
| `WorkTaskStatusChanged` | `['worktasks', projectId]` |
| `AgentStatusChanged` | `['agentInstances', projectId]` |
| `RevisionGateResolved` | `['gates', projectId]` |
| `TaskAttemptCompleted` | `['attempts', workTaskId]` |
| `TaskAttemptResolved` | `['attempts', workTaskId]` |

### Zustand store slices

```typescript
// Connection slice
interface ConnectionState {
  status: 'connecting' | 'connected' | 'disconnected'
  setStatus: (s: ConnectionState['status']) => void
}

// Notification slice
interface Notification { id: string; message: string; variant: 'info' | 'success' | 'error' }
interface NotificationState {
  notifications: Notification[]
  push: (n: Omit<Notification, 'id'>) => void
  dismiss: (id: string) => void
}
```

---

## Implementation Scope — What must be done

- [x] Create `src/store/useAppStore.ts` with Zustand store combining connection slice
  and notification slice
- [x] Create `src/context/SignalRContext.tsx`:
  - Builds `HubConnection` with `withAutomaticReconnect()`
  - Sets `status` in Zustand on connection lifecycle events
  - Registers `"ReceiveEvent"` handler that dispatches by `type`
  - Starts connection on mount, stops on unmount
  - Exposes `useSignalR()` hook returning `{ status }`
- [x] Wire event dispatch for all 6 event types (invalidation + optional toast)
- [x] Create `src/components/ui/ToastContainer.tsx`:
  - Reads from Zustand notification queue
  - Renders stack of dismissable toast cards in bottom-right
  - Auto-dismiss after 4 s using `setTimeout`
  - Slide-in animation with Tailwind `transition` classes
- [x] Update `ConnectionDot` in top bar to read `status` from Zustand
  (gray = disconnected, amber = connecting, green = connected)
- [x] Wrap root router with `SignalRProvider` in `src/main.tsx`
- [x] Add `ToastContainer` to `AppLayout`
- [x] `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement optimistic cache updates from events — invalidate and refetch only
- Do not add per-project room/group joining on the SignalR hub

---

## Test Expectations

- Unit tests required for: Zustand store slices (push/dismiss notification, setStatus)
- Edge cases to cover: push beyond max 5 notifications drops the oldest

---

## Open Questions

- None.
