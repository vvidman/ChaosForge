---
category: specs
title: "SignalR Notification Handlers — Domain Events to Frontend"
branch: "signalr-notify"
status: done
date: "2026-04-12"
related_domain: []
related_adr: [009-signalr-events]
---

# Feature Spec — SignalR Notification Handlers — Domain Events to Frontend

<!-- Reference this file in the implementation agent with: implement @docs/specs/30-signalr-notification-handlers.md -->

---

## Context

The hub exists (spec 29) but nothing broadcasts to it yet. This spec implements MediatR
`INotificationHandler<T>` implementations that listen for domain events and push them
to all connected SignalR clients. This is the final link in the event chain:
entity → domain event → MediatR publish → SignalR broadcast → React frontend.
Depends on: spec 29 (hub), spec 18 (dispatcher), specs 26–27 (events fired).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- All notification handlers live in `Infrastructure/Hubs/Notifications/`.
- They are `internal sealed` classes.
- Each handler injects `IHubContext<ChaosForgeHub>` and broadcasts to `Clients.All`.
- Message format (all messages):
  ```json
  { "type": "<EventName>", "payload": { <fields> } }
  ```
  Use a helper record `SignalRMessage(string Type, object Payload)` serialized with
  `System.Text.Json`. Define this in `Infrastructure/Hubs/SignalRMessage.cs`.
- Handler registration: automatic via MediatR assembly scan in `AddApplication()` —
  BUT these handlers live in Infrastructure, not Application. Therefore they must be
  registered explicitly in `Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
  ```
  Add this call alongside the existing Application `AddMediatR` call.

- Events to handle and their broadcast payloads:

  **`ProjectStatusChangedEvent` → broadcast `"ProjectStatusChanged"`**
  ```json
  { "projectId": "...", "oldStatus": "RequirementsPhase", "newStatus": "ArchitecturePhase" }
  ```

  **`AgentInstanceStatusChangedEvent` → broadcast `"AgentStatusChanged"`**
  ```json
  { "agentInstanceId": "...", "oldStatus": "Idle", "newStatus": "Working" }
  ```

  **`WorkTaskStatusChangedEvent` → broadcast `"WorkTaskStatusChanged"`**
  ```json
  { "workTaskId": "...", "oldStatus": "Backlog", "newStatus": "InProgress" }
  ```

  **`RevisionGateResolvedEvent` → broadcast `"RevisionGateResolved"`**
  ```json
  { "revisionGateId": "...", "projectId": "...", "action": "Accept" }
  ```

  **`TaskAttemptCompletedEvent` → broadcast `"TaskAttemptCompleted"`**
  ```json
  { "taskAttemptId": "...", "workTaskId": "...", "type": "Development" }
  ```

  **`TaskAttemptResolvedEvent` → broadcast `"TaskAttemptResolved"`**
  ```json
  { "taskAttemptId": "...", "workTaskId": "...", "result": "Approved" }
  ```

- Enum values are serialized as strings (configure `JsonStringEnumConverter` in the
  SignalR JSON serialization options).

---

## Implementation Scope — What must be done

- [x] Create `Infrastructure/Hubs/SignalRMessage.cs`:
  ```csharp
  internal sealed record SignalRMessage(string Type, object Payload);
  ```

- [x] Create one handler per event in `Infrastructure/Hubs/Notifications/`:
  - `ProjectStatusChangedSignalRHandler`
  - `AgentStatusChangedSignalRHandler`
  - `WorkTaskStatusChangedSignalRHandler`
  - `RevisionGateResolvedSignalRHandler`
  - `TaskAttemptCompletedSignalRHandler`
  - `TaskAttemptResolvedSignalRHandler`

  Each handler:
  - Implements `INotificationHandler<TEvent>`
  - Injects `IHubContext<ChaosForgeHub>`
  - Constructs a `SignalRMessage` with the appropriate type string and payload
  - Calls `await _hubContext.Clients.All.SendAsync("ReceiveEvent", message, cancellationToken)`

- [x] Configure SignalR JSON serialization in `DependencyInjection.cs`:
  ```csharp
  services.AddSignalR().AddJsonProtocol(options =>
  {
      options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter());
  });
  ```

- [x] Register Infrastructure MediatR handlers in `Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddMediatR(cfg =>
      cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
  ```

- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement per-project client groups or filtering
- Do not implement authentication on the hub connection
- Do not implement client-to-server messages

---

## Test Expectations

- Unit tests required for:
  - Each handler: verify `IHubContext.Clients.All.SendAsync` is called with the correct
    method name and a `SignalRMessage` with the correct `Type` string
  - One representative handler: verify payload fields are correctly mapped from the event
- Edge cases to cover: handler does not throw if `SendAsync` fails (log and swallow —
  SignalR delivery failure must not crash the event chain)

---

## Open Questions

- None. The frontend JavaScript client will subscribe to `"ReceiveEvent"` and dispatch
  based on the `type` field. This is a deliberate single-channel design — simpler than
  one SignalR method per event type.
