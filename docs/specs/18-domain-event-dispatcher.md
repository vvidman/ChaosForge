---
category: specs
title: "Domain Event Dispatcher"
branch: "evt-dispatch"
status: ready
date: "2026-04-07"
related_domain: []
related_adr: [003-background-service-workers, 009-signalr-events]
---

# Feature Spec — Domain Event Dispatcher

<!-- Reference this file in the implementation agent with: implement @docs/specs/18-domain-event-dispatcher.md -->

---

## Context

`IDomainEventDispatcher` was defined in the Domain layer (spec 03) but never implemented.
Entities raise domain events during state transitions, but nothing dispatches them —
the events are silently dropped after `SaveChangesAsync`. Without a dispatcher, the system
cannot react to state changes (e.g. notify the frontend via SignalR, trigger orchestration
logic). This spec implements the dispatcher in Infrastructure and wires it into the
`SaveChangesAsync` flow. Depends on: spec 03 (domain events), spec 08 (EF Core), spec 09
(repositories).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none — `IDomainEventDispatcher` already exists in Domain

---

## Architecture Decisions

- The dispatcher implementation lives in `Infrastructure/Events/DomainEventDispatcher.cs`.
- It is `internal sealed`.
- Dispatch strategy: after `base.SaveChangesAsync` completes successfully, the dispatcher
  collects all domain events from all tracked entities, dispatches them, then clears them.
  Events are dispatched **after** the database write to ensure the persisted state is
  consistent when handlers react.
- `AppDbContext.SaveChangesAsync` is overridden to: (1) call `base.SaveChangesAsync`,
  (2) collect domain events from change tracker, (3) dispatch, (4) clear. If dispatch
  fails, the exception propagates — no silent swallowing.
- The dispatcher iterates `IDomainEvent` instances and publishes each via MediatR's
  `IPublisher.Publish`. This means each domain event becomes a MediatR notification.
  Domain event handler classes implement `INotificationHandler<TEvent>`.
- In this spec, no concrete `INotificationHandler` implementations are written — only the
  dispatcher infrastructure. The first real handlers come in later specs (SignalR, orchestration).
- `IDomainEventDispatcher` is registered in `Infrastructure/DependencyInjection.cs` as
  `Scoped`.
- `AppDbContext` must inject `IDomainEventDispatcher` via constructor injection.

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/Events/DomainEventDispatcher.cs`:
  ```csharp
  internal sealed class DomainEventDispatcher(IPublisher publisher) : IDomainEventDispatcher
  {
      public async Task DispatchAsync(
          IReadOnlyList<IDomainEvent> events,
          CancellationToken cancellationToken = default)
      {
          foreach (var domainEvent in events)
          {
              await publisher.Publish(domainEvent, cancellationToken);
          }
      }
  }
  ```

- [ ] Update `AppDbContext`:
  - Add `IDomainEventDispatcher _dispatcher` via constructor injection (primary constructor)
  - Override `SaveChangesAsync` to:
    1. Call `await base.SaveChangesAsync(cancellationToken)`
    2. Collect all `IDomainEvent` instances from `ChangeTracker.Entries<EntityBase<Guid>>()`
       where the entity has domain events
    3. Call `await _dispatcher.DispatchAsync(events, cancellationToken)`
    4. Call `ClearDomainEvents()` on each entity
    5. Return the int from step 1

- [ ] Update `AppDbContextFactory` to pass a no-op dispatcher (for EF CLI tooling):
  ```csharp
  // Use a NullDomainEventDispatcher or pass a Substitute for design-time factory
  ```
  Simplest approach: create `NullDomainEventDispatcher : IDomainEventDispatcher` in
  `Infrastructure/Events/` that does nothing — used by the design-time factory only.

- [ ] Register in `Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
  ```

- [ ] Run `dotnet build` — zero warnings, zero errors
- [ ] Run `dotnet test` — all existing tests must still pass (no handler exists yet,
  so Publish calls will be no-ops in unit tests — verify NSubstitute mocks are not broken)

---

## Out of Scope — What must NOT be done

- Do not implement any `INotificationHandler<T>` in this spec
- Do not add SignalR in this spec — that is a future spec
- Do not change entity or domain event classes

---

## Test Expectations

- Unit tests required for:
  - `DomainEventDispatcher`: verify `IPublisher.Publish` is called once per event, in order
  - `AppDbContext.SaveChangesAsync` integration: verify events are cleared after dispatch
    (use an in-memory or SQLite in-memory EF context)
- Edge cases to cover: entity with no domain events — dispatcher is not called (or called
  with empty list and produces no Publish calls)

---

## Open Questions

- None.
