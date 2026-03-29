---
category: adr
id: "003"
title: "BackgroundService for agent workers"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [agent-design, solid]
---

# ADR-003: BackgroundService for agent workers

## Status
`accepted`

## Context
Multiple agent roles (Developer 1..N, Tester 1..N, Reviewer 1..N) must pick up tasks
independently and in parallel. Agent execution is long-running and asynchronous. The
infrastructure must run in-process with the .NET API host without requiring external
message brokers or actor frameworks.

## Decision
Each agent role is implemented as a hosted `BackgroundService` that polls for available
tasks on a configured interval. Multiple instances of multi-slot roles can run concurrently.
`AgentWorkerService` is transport-agnostic: it dispatches MediatR commands and emits domain
events; it has no knowledge of SignalR, HTTP, or any UI concern.

## Consequences

**Positive:**
- Runs in-process, no external broker dependency
- Parallelism is natural: multiple worker instances pick up tasks simultaneously
- Transport layer is fully decoupled — `IDomainEventDispatcher` abstracts delivery
- Easy to test in isolation: inject a mock `ILLMProvider` and a fake task queue

**Trade-offs:**
- Polling introduces latency (acceptable for this use case)
- No built-in dead-letter or retry queue — failures must be handled explicitly in the worker
- Scaling beyond a single process requires extracting to a message queue (but this is a
  deliberate future decision, not a current requirement)

## Alternatives Considered

### Option A: Actor model (Akka.NET / Microsoft Orleans)
True message-passing concurrency, built-in supervision and retry.
Rejected because the operational complexity and learning curve are disproportionate to a
solo hobby project. Actors are the right answer if ChaosForge ever becomes distributed;
BackgroundService is sufficient for single-process operation.

### Option B: Message queue (RabbitMQ / Azure Service Bus)
Durable queuing, guaranteed delivery, horizontal scaling.
Rejected because it requires an external infrastructure dependency. ChaosForge is designed
to run locally without Docker or cloud services. If durability becomes a requirement, this
ADR should be revisited.

## References
- See `agent-design.md` for role definitions and task lifecycle
- See ADR-009 for SignalR event delivery, which is intentionally decoupled from this worker
