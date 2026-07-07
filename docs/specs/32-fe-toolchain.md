---
category: specs
title: "Frontend Toolchain and Design System"
branch: "fe-toolchain"
status: done
date: "2026-04-21"
related_domain: []
related_adr: [009-signalr-events]
---

# Feature Spec — Frontend Toolchain and Design System

<!-- Reference this file in the implementation agent with: implement @docs/specs/32-fe-toolchain.md -->

---

## Context

The React frontend does not exist yet. This spec scaffolds the entire frontend project,
establishes the toolchain, configures the design system, and produces a shell application
with routing and a persistent layout. Every subsequent frontend spec builds on this
foundation. Nothing is displayed to the user yet beyond the shell — content comes in
later specs.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- **Scaffold:** run `npm create vite@latest web -- --template react-ts` from inside
  the `ChaosForge/` directory. The result lives at `ChaosForge/web/`, alongside
  `src/`, `tests/`, and `docs/`.
- **Package manager:** npm.
- **Styling:** Tailwind CSS v3 with the `@tailwindcss/typography` plugin. No CSS-in-JS.
  Configure `tailwind.config.ts` with a custom color palette (see Design Tokens below).
- **Component library:** shadcn/ui (Radix UI primitives + Tailwind). Install components
  individually as needed. Do not install the full library upfront.
- **Routing:** React Router v6 (`react-router-dom`). Use `createBrowserRouter` with a
  root layout route.
- **Icons:** Lucide React (`lucide-react`). No other icon library.
- **State management:** React Query v5 (`@tanstack/react-query`) for server state.
  Zustand (`zustand`) for client-only global state (SignalR connection status,
  notification queue). No Redux.
- **HTTP client:** Axios (`axios`) with a pre-configured instance (base URL, default
  headers, error interceptor).
- **SignalR client:** `@microsoft/signalr`. Managed in a dedicated hook — not imported
  directly in components.
- **Code quality:** ESLint with `eslint-config-prettier`, Prettier. TypeScript strict mode.
- **Fonts:** Inter (Google Fonts, loaded via `index.html` `<link>`).
- **No SSR**, no Next.js — plain Vite SPA.

### Design Tokens (Tailwind config)

```typescript
colors: {
  forge: {
    50:  '#f0f4ff',
    100: '#e0e9ff',
    500: '#4f6ef7',  // primary brand
    600: '#3b54d9',
    700: '#2c3eb5',
    900: '#1a2470',
  },
  surface: {
    DEFAULT: '#0f1117',   // page background
    card:    '#1a1d27',   // card background
    border:  '#2a2d3e',   // borders
    hover:   '#22263a',   // hover state
  },
  status: {
    idle:      '#6b7280',
    working:   '#4f6ef7',
    blocked:   '#ef4444',
    finished:  '#22c55e',
    done:      '#22c55e',
    pending:   '#f59e0b',
    approved:  '#22c55e',
    rejected:  '#ef4444',
  },
}
```

### Root layout

- Persistent left sidebar (240 px, collapsible to icon-only at ≤ 768 px)
- Top bar with project selector and connection status indicator
- Main content area with `<Outlet />`
- Sidebar navigation items: Projects, Active Sprint, Agent Monitor, History

### Route structure

```
/                        → redirect to /projects
/projects                → ProjectListPage
/projects/:id            → ProjectDetailPage (nested: /overview, /requirements, /sprint, /agents, /history)
/projects/:id/gate       → RevisionGatePage (full-screen modal route)
```

---

## Implementation Scope — What must be done

- [x] Scaffold Vite + React + TypeScript project at `ChaosForge/web/` by running
  `npm create vite@latest web -- --template react-ts` from within `ChaosForge/`
- [x] Install and configure Tailwind CSS with custom design tokens above
- [x] Install: `react-router-dom`, `@tanstack/react-query`, `zustand`, `axios`,
  `@microsoft/signalr`, `lucide-react`, `clsx`, `tailwind-merge`
- [x] Install shadcn/ui base: `Button`, `Badge`, `Card`, `Dialog`, `Separator`,
  `Tooltip`, `Skeleton`, `ScrollArea`
- [x] Configure ESLint + Prettier
- [x] Configure TypeScript strict mode (`"strict": true` in `tsconfig.json`)
- [x] Create `src/lib/api.ts` — Axios instance with:
  - `baseURL: import.meta.env.VITE_API_URL` (default `http://localhost:5143`)
  - Request interceptor: add `Content-Type: application/json`
  - Response interceptor: extract `error` field from `{ error: string }` responses
    and throw a typed `ApiError`
- [x] Create `src/lib/queryClient.ts` — React Query client with:
  - `staleTime: 30_000`
  - `retry: 1`
  - Global `onError` that pushes to Zustand notification queue
- [x] Create root layout component `src/layouts/AppLayout.tsx`:
  - Sidebar with nav items (icons + labels)
  - Top bar with connection status dot (gray = disconnected, green = connected)
  - `<Outlet />` in main area
- [x] Create `src/router.tsx` with `createBrowserRouter`, root layout, and placeholder
  page components for all routes (they render only a heading for now)
- [x] Wire `QueryClientProvider`, `RouterProvider` in `src/main.tsx`
- [x] Create `web/.env.example` inside `ChaosForge/web/`:
  ```
  VITE_API_URL=http://localhost:5143
  VITE_HUB_URL=http://localhost:5143/hubs/chaosforge
  ```
- [x] `npm run build` passes with zero TypeScript errors

---

## Out of Scope — What must NOT be done

- Do not implement any real page content — placeholder headings only
- Do not implement SignalR connection logic — that is spec 34
- Do not implement any API calls beyond the Axios instance setup

---

## Test Expectations

- Unit tests required for: none in this spec (toolchain setup, no logic)
- Edge cases to cover: n/a

---

## Open Questions

- None.
