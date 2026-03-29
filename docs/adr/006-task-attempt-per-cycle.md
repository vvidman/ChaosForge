---
category: adr
id: "006"
title: "TaskAttempt record per dev/review/test cycle"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [agent-design]
---

# ADR-006: TaskAttempt record per dev/review/test cycle

## Status
`accepted`

## Context
A Developer, Tester, or Reviewer may execute multiple cycles on the same task if rejected.
Each new cycle must receive the previous output and the rejection reason as prompt context —
without this, the agent repeats the same mistake. The full history of attempts must be
queryable (for the sprint board and for debugging agent behavior).

## Decision
Every dev/review/test cycle creates a new `TaskAttempt` entity storing: attempt number,
agent role, full output, and the rejection reason that prompted this attempt (null for the
first attempt). The attempt is persisted before the gate decision is recorded. The next
agent cycle receives the previous `TaskAttempt` output + rejection notes injected into the prompt.

## Consequences

**Positive:**
- Agents learn from rejection within a task lifecycle without maintaining in-memory state
- Full attempt history is auditable and queryable
- Prompt construction is deterministic: always previous attempt + rejection reason

**Trade-offs:**
- Storage grows with rejection cycles — acceptable for SQLite/hobby scale
- Prompt injection logic must be kept in sync with the TaskAttempt schema

## Alternatives Considered

### Option A: Overwrite task output in place, keep only latest
Simplest persistence model. Store rejection reason as a task field.
Rejected because it destroys the history of what the agent tried, making it impossible to
debug why an agent kept producing the same wrong output across cycles.

### Option B: Store full conversation history per task
Append every exchange as a conversation turn; feed the entire history to the next cycle.
Rejected because prompt size grows unboundedly with each rejection cycle. Injecting only
the previous attempt output + rejection reason provides sufficient context while keeping
prompt size predictable and bounded.

## References
- See `agent-design.md` for task lifecycle and attempt flow
- See ADR-005 for RevisionGate, which records the human decision that closes each attempt
