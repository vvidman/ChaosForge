---
category: specs
title: "CR Fix: Env var naming inconsistency in SignalRContext"
branch: "fix-env-var-naming"
status: ready
date: "2026-04-25"
related_domain: []
related_adr: []
---

# Feature Spec — CR Fix: Env var naming inconsistency in SignalRContext

<!-- Reference this file in the implementation agent with: implement @docs/specs/cr-fix-02-env-var-naming.md -->

---

## Context

`.env.example` defines two variables: `VITE_API_URL` and `VITE_HUB_URL`.

`src/lib/api.ts` correctly uses `VITE_API_URL`.

`src/context/SignalRContext.tsx` uses `VITE_API_BASE_URL` — a variable that is not
defined anywhere and will always fall back to the hardcoded default. If a user sets
`VITE_API_URL` to a custom value, the SignalR connection will still use `localhost:5143`
instead.

Additionally, `VITE_HUB_URL` is defined in `.env.example` but never used — the hub URL
is constructed manually by appending `/hubs/chaosforge` to the base URL.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `SignalRContext.tsx` must use `VITE_HUB_URL` directly (already defined in `.env.example`),
  not construct the URL from a base URL variable.
- `VITE_API_BASE_URL` must be removed — it is unused and confusing.
- `api.ts` continues to use `VITE_API_URL` unchanged.

---

## Implementation Scope — What must be done

- [ ] Update `src/context/SignalRContext.tsx`:
  ```typescript
  // Remove:
  const BASE_URL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:5143'
  const HUB_URL = `${BASE_URL}/hubs/chaosforge`

  // Replace with:
  const HUB_URL = import.meta.env.VITE_HUB_URL ?? 'http://localhost:5143/hubs/chaosforge'
  ```

- [ ] Verify `.env.example` already has `VITE_HUB_URL` defined (it does — no change needed)

- [ ] Run `npm run build` — zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not modify `src/lib/api.ts` — its `VITE_API_URL` usage is correct
- Do not add `VITE_API_BASE_URL` anywhere

---

## Test Expectations

- Unit tests required for: none (env var wiring, no logic)
- Edge cases to cover: n/a

---

## Open Questions

- None.
