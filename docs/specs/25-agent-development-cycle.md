---
category: specs
title: "Development Cycle Agent Workers"
branch: "agent-dev"
status: ready
date: "2026-04-12"
related_domain: [WorkTask, TaskAttempt, AgentInstance]
related_adr: [003-background-service-workers, 004-illmprovider-abstraction, 006-task-attempt-per-cycle]
---

# Feature Spec — Development Cycle Agent Workers

<!-- Reference this file in the implementation agent with: implement @docs/specs/25-agent-development-cycle.md -->

---

## Context

The four Development phase agents — Developer, Tester, Reviewer, TechnicalWriter — each
operate on individual WorkTasks rather than at the project level. They share the same
polling-and-pick-up pattern: find an eligible task, claim it, call the LLM, and advance
the task status. They are grouped in one spec because their structure is nearly identical;
only the eligible status, AttemptType, LLM prompt, and resulting status transition differ.
Depends on: spec 21 (AgentWorkerBase).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- Four workers: `DeveloperWorker`, `TesterWorker`, `ReviewerWorker`, `TechnicalWriterWorker`.
  All `sealed`, all extend `AgentWorkerBase`.
- Active phase for all four: `ProjectStatus.Development` only.
- Task eligibility by role:

  | Worker | Picks up tasks in status | Produces AttemptType | Transitions to |
  |---|---|---|---|
  | Developer | `Backlog` (with SprintId set) | `Development` | `InProgress` → `InReview` |
  | Reviewer | `InReview` | `Review` | `InTesting` (approve) or `Backlog` (reject) |
  | Tester | `InTesting` | `Testing` | `InDocumentation` (pass) or `Backlog` (reject) |
  | TechnicalWriter | `InDocumentation` | `Documentation` | `Done` |

- Per-cycle execution flow (identical for all four):
  1. Resolve idle agent instance for this role
  2. Find first eligible WorkTask (correct status, `SprintId` not null for Developer)
  3. If no eligible task: idle — skip cycle
  4. `StartAgentWorkCommand(agentInstanceId, taskId)`
  5. Fetch most recent rejected `TaskAttempt` for this task and AttemptType (if any)
  6. Build prompt using `AgentPromptBuilder.BuildWithPriorAttempt`
  7. Call LLM via `ILlmProviderSelector.GetProviderForRole(role).CompleteAsync`
  8. `CreateTaskAttemptCommand` → get new `attemptId`
  9. `CompleteTaskAttemptCommand(attemptId, llmOutput)`
  10. Evaluate and apply status transition (see table above)
  11. `FinishAgentWorkCommand(agentInstanceId)`
- Reviewer and Tester decisions: in this spec, the worker always **approves** — rejection
  logic requires a second LLM call to evaluate quality, which is a future enhancement.
  Document this as a known simplification: Reviewer and Tester always call `ApproveTaskAttemptCommand`
  and advance the task. Human rejection via the API remains available.
- Developer: after LLM output is saved, calls `StartWorkTaskCommand` then `SendWorkTaskToReviewCommand`.
- TechnicalWriter: after output saved, calls `PassWorkTaskTestingCommand`... wait — no.
  TechnicalWriter picks up `InDocumentation` and calls `CompleteWorkTaskCommand`.
- If the LLM call throws: log, call `FinishAgentWorkCommand` (releases the agent), do NOT
  mark the task as failed — it stays in its current status and will be picked up again on
  the next cycle.
- Provider: Developer, Tester, Reviewer, TechnicalWriter all use LlamaSharp (local) per
  the role-to-provider mapping in `LlmProviderSelector`.

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/Agents/DeveloperWorker.cs` (`sealed`, extends `AgentWorkerBase`):
  - Picks `Backlog` tasks with non-null `SprintId`
  - After LLM output saved: `StartWorkTaskCommand` → `SendWorkTaskToReviewCommand`
  - System prompt: instruct LLM to implement the task description, output Markdown code

- [ ] Create `Infrastructure/Agents/ReviewerWorker.cs` (`sealed`, extends `AgentWorkerBase`):
  - Picks `InReview` tasks
  - Always approves in this spec: `ApproveTaskAttemptCommand` → `ApproveWorkTaskCommand`
  - System prompt: code review instructions

- [ ] Create `Infrastructure/Agents/TesterWorker.cs` (`sealed`, extends `AgentWorkerBase`):
  - Picks `InTesting` tasks
  - Always passes in this spec: `ApproveTaskAttemptCommand` → `PassWorkTaskTestingCommand`
  - System prompt: test case generation instructions

- [ ] Create `Infrastructure/Agents/TechnicalWriterWorker.cs` (`sealed`, extends `AgentWorkerBase`):
  - Picks `InDocumentation` tasks
  - After output saved: `CompleteWorkTaskCommand`
  - System prompt: documentation writing instructions

- [ ] Each worker: prior attempt fetch — query `GetTaskAttemptsByWorkTaskIdQuery`, filter by
  matching `AttemptType`, take most recent with `Result = Rejected`, pass to `AgentPromptBuilder`

- [ ] Register all four as hosted services in `Infrastructure/DependencyInjection.cs`:
  ```csharp
  services.AddHostedService<DeveloperWorker>();
  services.AddHostedService<ReviewerWorker>();
  services.AddHostedService<TesterWorker>();
  services.AddHostedService<TechnicalWriterWorker>();
  ```

- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement LLM-based quality evaluation for Reviewer/Tester — always approve in
  this spec; rejection is a future enhancement
- Do not implement multi-instance parallelism configuration — multiple `AddHostedService`
  calls of the same type achieve this naturally if needed, but it is not in scope here

---

## Test Expectations

- Unit tests required for:
  - `DeveloperWorker`: no eligible task → skips cycle; eligible task → correct command sequence
    (StartWork → CreateAttempt → CompleteAttempt → StartWorkTask → SendToReview → FinishWork)
  - `DeveloperWorker`: prior rejected attempt exists → prompt includes rejection context
  - `ReviewerWorker`: eligible task → approve path command sequence
  - `TechnicalWriterWorker`: eligible task → complete path command sequence
  - LLM throws → agent calls `FinishAgentWorkCommand`, task status unchanged
- Edge cases to cover: Developer picks `Backlog` task with null `SprintId` → skips it

---

## Open Questions

- Auto-reject logic (Reviewer/Tester calling LLM to evaluate quality before deciding)
  is explicitly deferred. When added, the worker will need a second LLM call and a
  branching decision. The current always-approve approach is the safe baseline for demo.
