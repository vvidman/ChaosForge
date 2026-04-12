---
category: specs
title: "SignalR Hub and Infrastructure"
branch: "signalr-hub"
status: ready
date: "2026-04-12"
related_domain: []
related_adr: [009-signalr-events]
---

# Feature Spec — SignalR Hub and Infrastructure

<!-- Reference this file in the implementation agent with: implement @docs/specs/29-signalr-hub.md -->

---

## Context

The frontend needs real-time updates as agents work. ADR-009 established that SignalR is
the transport layer and that workers must never reference it directly. This spec implements
the hub itself and wires ASP.NET Core SignalR into the application. The hub is a passive
relay — it has no business logic. The notification handlers that broadcast events come
in spec 30. Depends on: spec 18 (DomainEventDispatcher), spec 26–27 (events fired).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `ChaosForgeHub` lives in `Infrastructure/Hubs/ChaosForgeHub.cs`. It extends
  `Hub` (no typed client interface in this spec — string-based method names are sufficient
  for the demo milestone).
- The hub has no methods that clients can call — it is receive-only from the client
  perspective. All messages are server-push.
- Hub route: `/hubs/chaosforge`
- CORS: allow any origin in Development, restricted in Production. In this spec, allow any
  origin — the React dev server runs on a different port.
- Client group model: all connected clients receive all events. No per-project filtering
  in this spec — a single broadcast to `All` is sufficient for a single-user tool.
- `ChaosForgeHub` must not inject any Application or Domain service. It is infrastructure only.
- SignalR package: use `Microsoft.AspNetCore.SignalR` (built into ASP.NET Core — no
  additional NuGet package needed beyond what is already referenced).
- Message format: all messages are broadcast as JSON with a `type` discriminator field:
  ```json
  { "type": "AgentStatusChanged", "payload": { ... } }
  ```
  The payload shape is defined per event in spec 30.

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/Hubs/ChaosForgeHub.cs`:
  ```csharp
  public sealed class ChaosForgeHub : Hub
  {
      // No client-callable methods in this spec.
      // All communication is server → client push via IHubContext<ChaosForgeHub>.
  }
  ```

- [ ] Update `Infrastructure/DependencyInjection.cs`:
  - Add `services.AddSignalR()` to `AddInfrastructure`

- [ ] Update `src/ChaosForge.API/Program.cs`:
  - After `app.UseHttpsRedirection()`:
    ```csharp
    app.UseCors(policy => policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
    ```
  - Map the hub:
    ```csharp
    app.MapHub<ChaosForgeHub>("/hubs/chaosforge");
    ```
  - Add `using ChaosForge.Infrastructure.Hubs;`

- [ ] Add `Microsoft.AspNetCore.SignalR.Client` NuGet package to
  `ChaosForge.Infrastructure.csproj` only if not already present. The server-side SignalR
  is part of the ASP.NET Core framework and needs no extra package — only the client
  SDK would need one, and that is a frontend concern.

- [ ] Run `dotnet build` — zero warnings, zero errors
- [ ] Verify hub is reachable: start the API and confirm that a WebSocket connection to
  `ws://localhost:5143/hubs/chaosforge` is accepted (use a tool like `wscat` or browser
  DevTools — no functional test required in this spec)

---

## Out of Scope — What must NOT be done

- Do not send any messages from the hub in this spec — that is spec 30
- Do not implement typed hub clients
- Do not implement authentication on the hub
- Do not add per-project client groups

---

## Test Expectations

- Unit tests required for: none — the hub has no logic to test
- Edge cases to cover: n/a

---

## Open Questions

- None.
