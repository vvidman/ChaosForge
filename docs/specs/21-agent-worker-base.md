---
category: specs
title: "Agent Worker Base Infrastructure"
branch: "agent-base"
status: ready
date: "2026-04-12"
related_domain: [AgentInstance, TaskAttempt]
related_adr: [003-background-service-workers, 004-illmprovider-abstraction]
---

# Feature Spec — Agent Worker Base Infrastructure

<!-- Reference this file in the implementation agent with: implement @docs/specs/21-agent-worker-base.md -->

---

## Context

Before any concrete agent can be implemented, the shared infrastructure must exist: an
abstract base class that handles the polling loop, agent status management, and prompt
construction. All seven agent roles will inherit from this base. Without it, each agent
would duplicate the same lifecycle boilerplate and the pattern would drift between roles.
This spec produces no runnable agent — it only produces the scaffolding.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: `IAgentWorker` (Application/Agents/)

---

## Architecture Decisions

- `IAgentWorker` lives in `Application/Agents/` — it defines the contract that the
  orchestration layer uses to control workers. One method: `Task ExecuteAsync(CancellationToken ct)`.
- `AgentWorkerBase` is an `abstract class` in `Infrastructure/Agents/`. It extends
  `BackgroundService` and implements the polling loop. Concrete agents override
  `ExecuteWorkAsync(AgentInstance instance, CancellationToken ct)`.
- Polling interval: configurable via `appsettings.json` key `"Agents:PollingIntervalMs"`
  (default 3000). Shared across all agent types.
- `AgentWorkerBase` injects `IServiceScopeFactory` — each poll cycle opens a new scope
  to resolve Scoped services (repositories, mediator). This is the standard pattern for
  long-running BackgroundServices that need Scoped dependencies.
- The base class is responsible for: resolving the correct `AgentInstance` for this role,
  checking that the project phase matches the role's active phase, calling the
  concrete `ExecuteWorkAsync`, and catching + logging unhandled exceptions without crashing.
- `AgentPromptBuilder` is a `static class` in `Infrastructure/Agents/` with pure methods
  for constructing prompts. No DI — it is a helper, not a service.
- Prompt injection rule (from `work-execution.md`): if a WorkTask has a previous `Rejected`
  `TaskAttempt`, the new prompt receives that attempt's `Output` + the relevant rejection
  note. The base class does NOT inject this — each concrete agent is responsible for fetching
  and injecting prior attempt context.

---

## Implementation Scope — What must be done

- [ ] Create `Application/Agents/IAgentWorker.cs`:
  ```csharp
  public interface IAgentWorker
  {
      Task ExecuteAsync(CancellationToken cancellationToken);
  }
  ```

- [ ] Create `Infrastructure/Agents/AgentWorkerOptions.cs`:
  ```csharp
  public sealed class AgentWorkerOptions
  {
      public int PollingIntervalMs { get; init; } = 3000;
  }
  ```

- [ ] Create `Infrastructure/Agents/AgentWorkerBase.cs` (`abstract`, extends `BackgroundService`):
  - Constructor injects: `IServiceScopeFactory`, `IOptions<AgentWorkerOptions>`,
    `ILogger<AgentWorkerBase>`
  - `ExecuteAsync` override: loop until cancellation, open scope per iteration,
    call `RunCycleAsync`, delay `PollingIntervalMs`, catch all exceptions and log (do not rethrow)
  - `abstract AgentRole Role { get; }` — concrete classes declare which role they serve
  - `abstract Task ExecuteWorkAsync(IServiceScope scope, AgentInstance instance, CancellationToken ct)`
  - `protected Task<AgentInstance?> ResolveIdleInstanceAsync(IServiceScope scope, CancellationToken ct)`
    — queries `IAgentInstanceRepository.GetByProjectIdAsync` and returns first `Idle` instance
    matching `Role`, or null if none available or project phase is wrong

- [ ] Create `Infrastructure/Agents/AgentPromptBuilder.cs` (`static class`):
  - `static string BuildWithPriorAttempt(string basePrompt, TaskAttempt? priorAttempt)`
    — if `priorAttempt` is null: return `basePrompt` unchanged
    — if not null: append section with prior output and rejection note
  - `static string FormatRejectionContext(TaskAttempt attempt)` — formats the prior attempt
    output and note into a readable prompt section

- [ ] Add `"Agents": { "PollingIntervalMs": 3000 }` to `appsettings.json`
- [ ] Register `AgentWorkerOptions` in `Infrastructure/DependencyInjection.cs`:
  `services.Configure<AgentWorkerOptions>(configuration.GetSection("Agents"))`
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not register any concrete BackgroundService here — that is in each agent spec
- Do not add any LLM call in this spec
- Do not implement phase-checking logic beyond checking `AgentInstance.Status == Idle`
  — phase gating comes in the orchestrator spec

---

## Test Expectations

- Unit tests required for:
  - `AgentPromptBuilder.BuildWithPriorAttempt`: null prior → base prompt unchanged;
    non-null prior → rejection context appended
- Edge cases to cover: prior attempt with null `ReviewNote` (not yet set) handled gracefully

---

## Open Questions

- None.
