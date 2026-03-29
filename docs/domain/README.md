---
category: domain
last_updated: "2026-03-29"
documents:
  - file: project-lifecycle.md
    covers: ["Project", "ProjectStatus", "phase transitions", "state machine", "Deadline", "aggregate root"]
  - file: requirements-pipeline.md
    covers: ["UseCase", "URS", "SRS", "BusinessAnalyst", "Architect", "ButterflyService", "HumanEditNote", "RevisionState", "requirements", "pipeline"]
  - file: work-execution.md
    covers: ["WorkTask", "TaskAttempt", "Sprint", "WorkTaskStatus", "AttemptType", "AttemptResult", "rejection notes", "Backlog", "InProgress", "InReview", "InTesting", "Done", "StoryPoints"]
  - file: revision-gate.md
    covers: ["RevisionGate", "RevisionGateType", "RevisionGateAction", "Accept", "EditAndAccept", "Reject", "RejectionReason", "ButterflyService", "AfterBA", "AfterArchitect", "AfterScrumMaster"]
  - file: agent-system.md
    covers: ["AgentSlot", "AgentInstance", "AgentRole", "AgentInstanceStatus", "singleton", "Developer", "Tester", "Reviewer", "TechnicalWriter", "PersonaName", "CurrentTaskId"]
---

# Domain Knowledge

Business domain knowledge for ChaosForge: entities, invariants, state machines, and business rules.
Load when working on a specific feature to understand the business intent behind the code.

**Note:** These documents describe *what* the domain rules are. For *why* specific design
decisions were made, see the ADRs. For *how* the architecture enforces these rules, see the
architecture documents.

## Documents

### `project-lifecycle.md`
`Project` aggregate root and `ProjectStatus` state machine. Phase transition rules and
which operations each phase permits.
Load when: working on project creation, phase transitions, or anything that checks current project status.

### `requirements-pipeline.md`
`UseCase → URS → SRS` pipeline. BA and Architect output structure, `HumanEditNote` propagation,
and ButterflyService trigger conditions.
Load when: working on BA or Architect agent output, URS/SRS entities, or butterfly propagation logic.

### `work-execution.md`
`Sprint`, `WorkTask`, and `TaskAttempt` entities. `WorkTaskStatus` state machine, rejection
flow, and the prompt injection rule for retry cycles.
Load when: working on task assignment, attempt creation, rejection handling, or sprint management.

### `revision-gate.md`
`RevisionGate` entity, Accept/EditAndAccept/Reject decision logic, ButterflyService trigger,
and gate lifecycle.
Load when: implementing or modifying revision gate resolution, phase unlock logic, or rejection reason handling.

### `agent-system.md`
`AgentSlot` configuration and `AgentInstance` identity and status. Role definitions,
singleton constraints, and instance lifecycle rules.
Load when: working on agent configuration, instance creation, role-based access rules, or agent status tracking.

---

## Adding a New Domain Document
Copy `_template.md` to a new file named after the domain area (e.g. `notifications.md`).
Add the new file to the `documents` list in this README's frontmatter.
