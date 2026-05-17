---
category: specs
title: "CR Fix: RevisionGatePage stale placeholder and hook typo"
branch: "fix-misc-stale"
status: ready
date: "2026-04-25"
related_domain: []
related_adr: []
---

# Feature Spec — CR Fix: RevisionGatePage stale placeholder and hook typo

<!-- Reference this file in the implementation agent with: implement @docs/specs/cr-fix-05-misc-stale.md -->

---

## Context

Two unrelated minor issues found during review, grouped here as a single small fix:

**1. `src/pages/RevisionGatePage.tsx` is a stale placeholder.**
The spec introduced `GatePage` at `src/pages/project/GatePage.tsx` as the actual
implementation. A leftover `RevisionGatePage.tsx` with a heading-only stub exists at
`src/pages/RevisionGatePage.tsx`. It is not referenced by the router, but it is dead
code that will cause confusion.

**2. `useMustateSendToReview` typo in `src/hooks/useWorkTasks.ts`.**
The exported function is named `useMustateSendToReview` (double `M` — `useMutate` got
corrupted to `usMutate`). It should be `useMutateSendToReview`. No component currently
imports it, but it will cause issues when connected.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `RevisionGatePage.tsx` is deleted — it is dead code.
- The typo in `useWorkTasks.ts` is renamed.

---

## Implementation Scope — What must be done

- [ ] Delete `src/pages/RevisionGatePage.tsx`
- [ ] Rename `useMustateSendToReview` → `useMutateSendToReview` in
  `src/hooks/useWorkTasks.ts`
- [ ] Update `src/hooks/index.ts` export if `useMustateSendToReview` is exported there
- [ ] Run `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not modify `GatePage.tsx` or the router

---

## Test Expectations

- Unit tests required for: none
- Edge cases to cover: n/a

---

## Open Questions

- None.
