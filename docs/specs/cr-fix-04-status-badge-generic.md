---
category: specs
title: "CR Fix: StatusBadge only handles ProjectStatus"
branch: "fix-status-badge-generic"
status: ready
date: "2026-04-25"
related_domain: []
related_adr: []
---

# Feature Spec — CR Fix: StatusBadge only handles ProjectStatus

<!-- Reference this file in the implementation agent with: implement @docs/specs/cr-fix-04-status-badge-generic.md -->

---

## Context

`StatusBadge` is typed to accept only `ProjectStatus` and its `statusConfig` record only
covers the 6 project lifecycle phases. The spec (spec 35) intended it as an app-wide
component for displaying any status — `WorkTaskStatus`, `AgentInstanceStatus`,
`AttemptResult`, etc. Currently the sprint board, agent monitor, and history tab cannot
use `StatusBadge` for task or agent statuses — each had to implement its own badge styling
inline, leading to inconsistency.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `StatusBadge` becomes generic over a string union — it accepts a `status: string` and
  looks it up in a unified `statusConfig` map.
- `statusConfig` is extended to cover all status values in the application.
- Callers that already use `StatusBadge` for `ProjectStatus` continue to work unchanged.
- The unified config lives in `src/components/ui/statusConfig.ts`.

### Extended status config map

Add entries for all missing status types:

```typescript
// WorkTaskStatus
Backlog:         { label: 'Backlog',          className: '...' }
InProgress:      { label: 'In Progress',      className: '...' }
InReview:        { label: 'In Review',        className: '...' }
InTesting:       { label: 'In Testing',       className: '...' }
InDocumentation: { label: 'In Documentation', className: '...' }
Done:            { label: 'Done',             className: '...' }

// AgentInstanceStatus
Idle:     { label: 'Idle',     className: '...' }
Working:  { label: 'Working',  className: '...' }
Blocked:  { label: 'Blocked',  className: '...' }
Finished: { label: 'Finished', className: '...' }

// AttemptResult
Pending:  { label: 'Pending',  className: '...' }
Approved: { label: 'Approved', className: '...' }
Rejected: { label: 'Rejected', className: '...' }
```

Use the existing `status.*` design tokens from `tailwind.config.ts` for colors:
- Working/InProgress → `status.working`
- Blocked/Rejected → `status.blocked`
- Done/Finished/Approved → `status.done`
- Pending → `status.pending`
- Idle → `status.idle`
- Backlog/InReview/InTesting/InDocumentation → muted gray tones

---

## Implementation Scope — What must be done

- [ ] Update `src/components/ui/statusConfig.ts` — add all new entries above with
  appropriate Tailwind classes using the design token colors
- [ ] Update `src/components/ui/StatusBadge.tsx`:
  - Change prop type from `status: ProjectStatus` to `status: string`
  - Add fallback for unknown status:
    ```typescript
    const config = statusConfig[status] ?? { label: status, className: 'border-surface-border text-gray-400' }
    ```
- [ ] Update `StatusBadge.test.tsx` to cover at least one `WorkTaskStatus` and one
  `AgentInstanceStatus` value

- [ ] Run `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not replace existing inline badge styling in `AttemptTypeBadge` or
  `AttemptResultBadge` — those components can remain as-is; the fix only ensures
  `StatusBadge` is usable for broader status types going forward

---

## Test Expectations

- Unit tests required for:
  - `StatusBadge` renders correct label and applies a non-empty className for each
    new status value (spot-check: `'Working'`, `'Blocked'`, `'Done'`, `'Pending'`)
  - `StatusBadge` renders with fallback for an unknown status string
- Edge cases to cover: unknown status string shows the raw status value as label

---

## Open Questions

- None.
