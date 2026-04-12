---
category: specs
title: "Architect Agent Worker"
branch: "agent-arch"
status: ready
date: "2026-04-12"
related_domain: [URS, SRS, WorkTask, RevisionGate, AgentInstance]
related_adr: [003-background-service-workers, 004-illmprovider-abstraction]
---

# Feature Spec — Architect Agent Worker

<!-- Reference this file in the implementation agent with: implement @docs/specs/23-agent-architect.md -->

---

## Context

The Architect activates during `ArchitecturePhase`. It reads all URS items produced by the
BusinessAnalyst, generates an SRS for each, then decomposes each SRS into WorkTasks. When
complete, it opens an `AfterArchitect` RevisionGate. This is the most complex singleton agent
because it produces two entity types (SRS + WorkTask) and must parse structured LLM output.
Depends on: spec 21 (AgentWorkerBase), spec 22 (BA pattern established).

---

## Domain Impact

- New or modified entity: none (creates SRS and WorkTask via existing commands)
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `ArchitectWorker` extends `AgentWorkerBase`, `Role = AgentRole.Architect`.
- Active phase: `ProjectStatus.ArchitecturePhase` only.
- Two-pass LLM strategy per URS:
  - **Pass 1 — SRS generation**: one LLM call per URS item → produces `TechnicalDescription`
  - **Pass 2 — WorkTask decomposition**: one LLM call per SRS item → produces a JSON list
    of tasks
- WorkTask decomposition prompt instructs the LLM to respond ONLY in JSON:
  ```json
  [
    { "title": "...", "description": "...", "storyPoints": 3 },
    ...
  ]
  ```
  Parse with `System.Text.Json`. If parsing fails, log the raw output and skip that SRS
  item — do not crash the cycle.
- `HumanEditNote` injection: if the corresponding URS has a non-null `HumanEditNote`
  (set by a previous `EditAndAccept`), prepend it to the SRS generation prompt as context.
- Prior rejection: if the previous `AfterArchitect` gate was `Rejected`, prepend the
  `RejectionReason` to the system prompt.
- Execution flow:
  1. Resolve idle Architect instance
  2. Check no open `AfterArchitect` gate
  3. Fetch all URS items for the project
  4. Mark agent `Working`
  5. For each URS: generate SRS via LLM → `CreateSRSCommand`
  6. For each SRS: decompose to WorkTasks via LLM → `CreateWorkTaskCommand` per task
  7. Build combined summary for gate output
  8. Open `AfterArchitect` RevisionGate
  9. Mark agent `Finished`
- Story points: parsed from LLM JSON. If missing or non-positive, default to 1.

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/Agents/ArchitectWorker.cs` (`sealed`, extends `AgentWorkerBase`)
- [ ] Override `Role => AgentRole.Architect`
- [ ] Define two system prompt constants (SRS generation, WorkTask decomposition)
- [ ] Implement SRS generation pass (one LLM call per URS, `CreateSRSCommand`)
- [ ] Implement WorkTask decomposition pass (one LLM call per SRS, JSON parse,
  `CreateWorkTaskCommand` per parsed task)
- [ ] Handle JSON parse failures: log raw LLM output + skip SRS, continue with next
- [ ] Implement prior `HumanEditNote` injection into SRS prompt
- [ ] Implement prior rejection context injection
- [ ] Register: `services.AddHostedService<ArchitectWorker>()`
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement sprint assignment here — that is ScrumMaster
- Do not implement ButterflyService here — spec 28
- Do not produce WorkTasks from SRS items whose URS has `Status = Rejected`

---

## Test Expectations

- Unit tests required for:
  - SRS generation: URS with `HumanEditNote` → note appears in prompt
  - WorkTask decomposition: valid JSON → correct number of `CreateWorkTaskCommand` calls
  - WorkTask decomposition: invalid JSON → logged, SRS skipped, cycle continues
  - LLM failure on SRS pass → agent Blocked, gate not opened
  - Missing/zero story points → defaults to 1
- Edge cases to cover: URS list is empty → worker skips cycle

---

## Open Questions

- None.
