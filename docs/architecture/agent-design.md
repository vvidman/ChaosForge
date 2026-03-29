---
category: architecture
principle: "Agent Design"
last_updated: "2026-03-29"
related_adr: [ADR-003, ADR-005, ADR-006]
---

# Agent Design

## What it is
Each agent role runs as an independent `BackgroundService` worker that polls for available
tasks, executes one LLM completion cycle, and emits domain events. Human judgment is injected
at `RevisionGate` checkpoints. `AgentWorkerService` is unaware of HTTP or SignalR.

## Why we apply it
True parallelism: multiple Developer workers pick up tasks simultaneously without coordination
logic. Decoupling transport (SignalR) from processing (BackgroundService) keeps each concern
independently replaceable.

## How we apply it

### Agent roles

| Role | Singleton | Input → Output |
|---|---|---|
| BusinessAnalyst | YES | UseCases → URS |
| Architect | YES | URS → SRS → Tasks |
| ScrumMaster | YES | Backlog → Sprint plan |
| Developer | 1..N | Task → implementation |
| Tester | 1..N | Code → unit tests + integration cases |
| Reviewer | 1..N | Code → accept / reject |
| TechnicalWriter | 1..N | Finished code → documentation |

### TaskAttempt
Every dev/review/test cycle creates a `TaskAttempt` record storing: previous attempt output,
rejection notes, and new output. Each new attempt receives the previous output + notes as
prompt context — prevents repetition without injecting full history.

### RevisionGate
After BA, Architect, and SM output — human chooses `Accept | EditAndAccept | Reject`.
Reject requires a mandatory reason (injected into next attempt prompt).
`EditAndAccept` triggers `ButterflyService`: modified output propagates downstream, potentially
invalidating SRS items or sprint assignments.

### Workflow phases
`Setup → RequirementsPhase → ArchitecturePhase → SprintPlanning → Development → Completed`

### Task lifecycle
`Backlog → InProgress → InReview → InTesting → InDocumentation → Done`
Reviewer or Tester may reject back to `Backlog` with notes.

## Rules
- **Must**: `AgentWorkerService` must not reference SignalR, `IHubContext`, `HttpContext`, or any HTTP concern
- **Must**: state changes go through domain entities and domain events — never direct DB writes
- **Must**: each `TaskAttempt` persists its full output *before* the gate decision is recorded
- **?** Does the worker know how events are delivered to the UI? If yes → extract to `IDomainEventDispatcher`.
- **?** Is the rejection reason captured and injected into the next prompt? If no → `TaskAttempt` is incomplete.

## Anti-patterns

### Worker with SignalR dependency
`AgentWorkerService` calling `IHubContext` directly — couples background processing to
transport layer. Use `IDomainEventDispatcher`; `SignalREventDispatcher` in Infrastructure
handles delivery.

### Stateless attempts
Not storing the previous attempt output — the next agent cycle cannot learn from rejection.
Full output must be persisted before the gate decision.

## References
- See `cqrs.md` — how workers issue Commands
- See `llm-strategy.md` — provider assignment per role
