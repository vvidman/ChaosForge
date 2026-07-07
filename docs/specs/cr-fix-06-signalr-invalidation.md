---
category: specs
title: "CR Fix: SignalR WorkTaskStatusChanged cache invalidation too broad"
branch: "fix-signalr-invalidation"
status: done
date: "2026-04-25"
related_domain: []
related_adr: [009-signalr-events]
---

# Feature Spec — CR Fix: SignalR WorkTaskStatusChanged cache invalidation too broad

<!-- Reference this file in the implementation agent with: implement @docs/specs/cr-fix-06-signalr-invalidation.md -->

---

## Context

In `SignalRContext.tsx`, the `WorkTaskStatusChanged` handler invalidates:

```typescript
queryClient.invalidateQueries({ queryKey: ['work-tasks'] })
```

This invalidates ALL work-task queries across ALL projects every time any task changes
status anywhere. On a busy project this causes constant refetches across all open views.
The backend event payload contains `workTaskId` — the projectId is not directly in
the payload, but `workTaskId` is sufficient to invalidate only the relevant queries.

Similarly, the `AgentStatusChanged` event correctly scopes to `projectId`, but the
`WorkTaskStatusChanged` event does not pass `projectId` in the payload (looking at the
backend SignalR handler in spec 30).

This fix has two parts:
1. Check what the backend actually sends in the `WorkTaskStatusChanged` payload.
2. If `projectId` is present: scope invalidation to that project. If not: invalidate
   more specifically using `workTaskId` at minimum rather than the entire `work-tasks`
   namespace.

Looking at the backend `WorkTaskStatusChangedSignalRHandler`, the payload is:
```json
{ "workTaskId": "...", "oldStatus": "...", "newStatus": "..." }
```
No `projectId`. The workTaskId alone is sufficient to target the specific queries.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Replace the broad `['work-tasks']` invalidation with targeted queries using `workTaskId`.
- Also invalidate the `by-project` query — but only if we can derive `projectId`.
  Since the event payload doesn't include it, we cannot scope it. The fallback is to
  invalidate `['work-tasks', 'by-project']` (the prefix) which is still narrower than
  invalidating all of `['work-tasks']`.

**Backend fix (small):** Add `projectId` to the `WorkTaskStatusChangedEvent` payload in
the SignalR handler so the frontend can scope invalidation correctly. This requires:
- Updating `WorkTaskStatusChangedSignalRHandler` in Infrastructure to include `projectId`
- Updating the `WorkTaskStatusChangedEvent` to carry `projectId` OR fetching it in the handler

The simplest approach: update the SignalR handler to also broadcast `projectId` (fetched
via the task's SRS → URS → UseCase chain, or add `ProjectId` to the domain event).

**Scope of this fix:** Given the chain lookup is complex, the pragmatic fix is:
1. Add `ProjectId` to `WorkTaskStatusChangedEvent` in the domain (it has `WorkTaskId`
   which can be looked up, but domain events shouldn't query — better to pass it in).
   Actually the cleanest: the WorkTask entity raises the event and knows only its own
   fields. The SRS/project chain is not available in the entity.
   **Decision: add `ProjectId` as an optional field passed by the orchestration handler
   when it knows it, leave null otherwise. SignalR handler broadcasts what it has.**

This is getting complex. **Practical decision for this spec:**
- Frontend: change `['work-tasks']` to `['work-tasks', 'by-project']` as the prefix
  invalidation — still narrower than the full namespace
- Backend: no change required in this fix pass — the full projectId plumbing is deferred
  to a future cleanup spec if needed

---

## Implementation Scope — What must be done

- [x] Update `src/context/SignalRContext.tsx`, `WorkTaskStatusChanged` case:
  ```typescript
  case 'WorkTaskStatusChanged': {
    const wTaskId = payload['workTaskId'] as string | undefined
    // Invalidate the specific task if we know its id
    if (wTaskId) {
      queryClient.invalidateQueries({ queryKey: ['work-tasks', wTaskId] })
    }
    // Invalidate all by-project queries (narrower than full ['work-tasks'])
    queryClient.invalidateQueries({ queryKey: ['work-tasks', 'by-project'] })
    push({ message: `Task → ${(payload['newStatus'] as string) ?? ''}`, variant: 'info' })
    pushAgentEvent({
      timestamp: new Date().toISOString(),
      type: 'WorkTaskStatusChanged',
      description: `Task status → ${(payload['newStatus'] as string) ?? 'unknown'}`,
    })
    break
  }
  ```

- [x] Run `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not modify backend domain events or SignalR handlers in this pass

---

## Test Expectations

- Unit tests required for: none (event dispatch wiring)
- Edge cases to cover: n/a

---

## Open Questions

- Adding `ProjectId` to `WorkTaskStatusChangedEvent` for full scoping is a future
  improvement. Current fix is a pragmatic middle ground.
