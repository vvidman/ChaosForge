---
category: adr
id: "009"
title: "SignalR for real-time agent event delivery"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [agent-design, clean-architecture]
---

# ADR-009: SignalR for real-time agent event delivery

## Status
`accepted`

## Context
The frontend (React) must receive agent progress events (task started, completed, gate
opened, phase changed) without polling. Agent workers run as background services and must
not be coupled to the transport layer. Event delivery must be in-process with the .NET host.

## Decision
Use ASP.NET Core SignalR for real-time push to the React frontend. `SignalREventDispatcher`
(Infrastructure) implements `IDomainEventDispatcher` (Domain) and calls `IHubContext<ChaosForgeHub>`.
Agent workers dispatch through `IDomainEventDispatcher` only — they have no reference to
SignalR. The hub is a passive relay: it receives domain events and forwards them to clients.

## Consequences

**Positive:**
- Workers are fully decoupled from transport: swapping SignalR for SSE requires only a
  new `IDomainEventDispatcher` implementation
- React receives push events without polling overhead
- In-process: no external message broker needed
- `ChaosForgeHub` has no business logic — it is a pure relay

**Trade-offs:**
- SignalR WebSocket connections add memory overhead per connected client (acceptable:
  single-user hobby tool)
- Frontend must handle reconnection logic if the WebSocket drops

## Alternatives Considered

### Option A: Client polling (REST endpoint for events)
Simplest implementation. Frontend polls `/api/events` on an interval.
Rejected because polling introduces latency between agent completion and UI update,
and generates unnecessary HTTP traffic during idle periods. The real-time nature of
watching agents work is a core UX goal.

### Option B: Server-Sent Events (SSE)
Unidirectional push, simpler than WebSocket, no special server library needed.
Rejected because ASP.NET Core SignalR is already in the stack (required for the hub),
and SSE would add a second real-time mechanism. SignalR's automatic transport negotiation
(WebSocket → SSE → long polling fallback) covers SSE as a fallback automatically.

## References
- See `agent-design.md` — IDomainEventDispatcher and the worker transport decoupling rule
- See ADR-003 for the BackgroundService design that requires this decoupling
