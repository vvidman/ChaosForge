---
category: specs
title: "ScrumMaster Agent Worker"
branch: "agent-sm"
status: ready
date: "2026-04-12"
related_domain: [WorkTask, AgentInstance, RevisionGate]
related_adr: [003-background-service-workers, 004-illmprovider-abstraction]
---

# Feature Spec — ScrumMaster Agent Worker

<!-- Reference this file in the implementation agent with: implement @docs/specs/24-agent-scrum-master.md -->

---

## Context

The ScrumMaster activates during `SprintPlanning`. It reviews the full backlog of WorkTasks,
uses the LLM to prioritize and assign them to a sprint (using a single Guid as sprint
identifier), then opens an `AfterScrumMaster` RevisionGate. After human approval, the
Development phase begins. Depends on: spec 21 (AgentWorkerBase).

---

## Domain Impact

- New or modified entity: none (mutates WorkTask via `AssignWorkTaskToSprintCommand`)
- New domain event: none
- New interface: none

---

## Architecture Decisions

- `ScrumMasterWorker` extends `AgentWorkerBase`, `Role = AgentRole.ScrumMaster`.
- Active phase: `ProjectStatus.SprintPlanning` only.
- Sprint model: ChaosForge does not have a `Sprint` entity yet. The ScrumMaster generates
  a single `Guid` at the start of its cycle and uses it as the `SprintId` for all tasks
  it assigns. This is a deliberate simplification — a full Sprint entity is out of scope
  for the demo milestone.
- LLM strategy: one call with all backlog tasks as a JSON list in the prompt. The LLM
  responds with an ordered JSON array of task IDs to include in the sprint:
  ```json
  { "sprintTaskIds": ["guid1", "guid2", ...] }
  ```
  The ScrumMaster assigns only the tasks in this list. Unparseable response → assign all
  tasks (fail-safe default).
- Execution flow:
  1. Resolve idle ScrumMaster instance
  2. Check no open `AfterScrumMaster` gate
  3. Fetch all `Backlog` WorkTasks for the project (via `GetWorkTasksByStatusQuery`)
  4. If no tasks, log warning and skip
  5. Mark agent `Working`
  6. Generate sprint `Guid` for this planning cycle
  7. Call LLM with task list → parse response → get ordered task IDs
  8. For each selected task ID: `AssignWorkTaskToSprintCommand`
  9. Build sprint plan summary for gate output
  10. Open `AfterScrumMaster` RevisionGate
  11. Mark agent `Finished`
- If LLM call fails or JSON unparseable: assign all tasks to the sprint (fail-safe), log warning.
- Deadline injection: if `Project.Deadline` is set, include it in the SM prompt for context.

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/Agents/ScrumMasterWorker.cs` (`sealed`, extends `AgentWorkerBase`)
- [ ] Override `Role => AgentRole.ScrumMaster`
- [ ] Define SM system prompt constant (task prioritization, sprint planning)
- [ ] Implement backlog fetch → JSON serialization for LLM prompt
- [ ] Implement LLM response JSON parse → extract task IDs
- [ ] Implement fail-safe: unparseable → assign all tasks
- [ ] Implement `AssignWorkTaskToSprintCommand` loop
- [ ] Register: `services.AddHostedService<ScrumMasterWorker>()`
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not create a Sprint entity in this spec
- Do not implement the `Start()` transition for tasks — that happens in the Development
  agent when it picks up a task
- Do not implement ButterflyService sprint revision here — spec 28

---

## Test Expectations

- Unit tests required for:
  - Happy path: LLM returns valid JSON with subset of task IDs →
    only those tasks receive `AssignWorkTaskToSprintCommand`
  - Fail-safe: LLM returns invalid JSON → all tasks assigned
  - No backlog tasks → worker skips, logs warning
  - LLM throws → agent Blocked, gate not opened
- Edge cases to cover: LLM returns task IDs not in the backlog → those IDs are silently skipped

---

## Open Questions

- Sprint entity is absent intentionally. If a future spec introduces Sprint, this worker
  will need to be updated to create one and link tasks to it.
