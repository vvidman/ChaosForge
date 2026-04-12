---
category: specs
title: "BusinessAnalyst Agent Worker"
branch: "agent-ba"
status: done
date: "2026-04-12"
related_domain: [UseCase, URS, RevisionGate, AgentInstance]
related_adr: [003-background-service-workers, 004-illmprovider-abstraction]
---

# Feature Spec — BusinessAnalyst Agent Worker

<!-- Reference this file in the implementation agent with: implement @docs/specs/22-agent-business-analyst.md -->

---

## Context

The BusinessAnalyst is the first agent to run in the pipeline. It activates during
`RequirementsPhase`, reads all UseCases for the project, generates a URS for each via
the LLM, persists the results, then opens an `AfterBA` RevisionGate for human review.
This is the first concrete agent and establishes the singleton agent pattern.
Depends on: spec 21 (AgentWorkerBase).

---

## Domain Impact

- New or modified entity: none (creates URS and RevisionGate via existing commands)
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `BusinessAnalystWorker` extends `AgentWorkerBase`, overrides `Role = AgentRole.BusinessAnalyst`.
- It is a `sealed` class in `Infrastructure/Agents/`.
- Active phase: `ProjectStatus.RequirementsPhase` only. If the project is in any other phase,
  `ResolveIdleInstanceAsync` returns null and the worker skips the cycle.
- Execution flow per cycle:
  1. Resolve idle `AgentInstance` for `BusinessAnalyst` role
  2. Check no open `AfterBA` gate exists for this project — if one is open, skip
  3. Fetch all `UseCase` entities for the project
  4. If no UseCases exist, log warning and skip
  5. Mark agent as `Working` via `StartAgentWorkCommand` (use a synthetic task id: `Guid.Empty`)
  6. For each UseCase, call `ILlmProviderSelector.GetProviderForRole(BusinessAnalyst).CompleteAsync`
     with the BA system prompt and the UseCase as user prompt
  7. Persist each URS via `CreateURSCommand`
  8. Build a combined summary of all URS output as `agentOutput`
  9. Open an `AfterBA` RevisionGate via `OpenRevisionGateCommand`
  10. Mark agent as `Finished` via `MarkAgentFinishedCommand`
- If step 6 (LLM call) throws, catch, log, mark agent `Blocked` via `BlockAgentCommand`, and abort cycle.
- System prompt for BA is a `const string` defined in the worker class.
- User prompt per UseCase: `Title + "\n" + Description`.
- Prior attempt injection: if a previous `AfterBA` gate was `Rejected`, fetch its
  `RejectionReason` and prepend it to the system prompt as context.
- `StartAgentWorkCommand` requires a real `WorkTaskId` per the domain model. Since BA works
  at project level (not task level), use a well-known sentinel: `Guid.Empty`. Document this
  as a known design gap to be revisited.

---

## Implementation Scope — What must be done

- [x] Create `Infrastructure/Agents/BusinessAnalystWorker.cs` (`sealed`, extends `AgentWorkerBase`)
- [x] Override `Role => AgentRole.BusinessAnalyst`
- [x] Implement `ExecuteWorkAsync` with the flow described above
- [x] Define BA system prompt constant:
  ```
  You are a Business Analyst. Your task is to produce a User Requirements Specification (URS)
  for the given use case. Write clearly and precisely. Focus on what the system must do,
  not how it does it. Output plain text only.
  ```
- [x] Register as `HostedService` in `Infrastructure/DependencyInjection.cs`:
  `services.AddHostedService<BusinessAnalystWorker>()`
- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement ButterflyService here — EditAndAccept propagation is spec 28
- Do not implement phase transition logic here — the orchestrator reacts to gate resolution
- Do not send SignalR notifications directly from the worker

---

## Test Expectations

- Unit tests required for:
  - Happy path: UseCases exist, LLM returns valid strings, URS created, gate opened,
    agent marked Finished — verify mediator receives correct commands in order
  - Skip path: open AfterBA gate already exists — verify no LLM call is made
  - LLM failure path: LLM throws — agent marked Blocked, gate NOT opened
- Edge cases to cover: no UseCases in project → worker skips cycle, logs warning

---

## Open Questions

- The `Guid.Empty` sentinel for `CurrentTaskId` is a design gap. Accepted for now — a
  future spec may introduce a phase-level `AgentTask` concept if needed.
