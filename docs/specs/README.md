---
category: specs
last_updated: "2026-04-12"
documents:
  - file: domain-entities.md
    title: "Domain Entities"
    status: done
    date: "2026-03-29"
  - file: 01-domain-unit-tests.md
    title: "Domain Unit Tests"
    status: done
    date: "2026-04-03"
  - file: 02-repository-interfaces.md
    title: "Repository Interfaces"
    status: done
    date: "2026-04-03"
  - file: 03-domain-events.md
    title: "Domain Events"
    status: done
    date: "2026-04-03"
  - file: 04-application-pipeline.md
    title: "Application Pipeline"
    status: done
    date: "2026-04-03"
  - file: 05-project-commands.md
    title: "Project Commands"
    status: done
    date: "2026-04-03"
  - file: 06-worktask-commands.md
    title: "WorkTask Commands"
    status: done
    date: "2026-04-03"
  - file: 07-revision-gate-commands.md
    title: "RevisionGate Commands"
    status: done
    date: "2026-04-03"
  - file: 08-ef-core-configuration.md
    title: "EF Core Configuration"
    status: done
    date: "2026-04-03"
  - file: 09-repository-implementations.md
    title: "Repository Implementations"
    status: done
    date: "2026-04-03"
  - file: 10-api-wiring.md
    title: "API Wiring and Project Endpoints"
    status: done
    date: "2026-04-03"
  - file: 11-project-queries.md
    title: "Project Queries"
    status: done
    date: "2026-04-07"
  - file: 12-worktask-revgate-queries.md
    title: "WorkTask and RevisionGate Queries"
    status: done
    date: "2026-04-07"
  - file: 13-usecase-urs-srs-queries.md
    title: "UseCase, URS and SRS Queries"
    status: done
    date: "2026-04-07"
  - file: 14-agent-attempt-queries.md
    title: "AgentSlot, AgentInstance and TaskAttempt Queries"
    status: done
    date: "2026-04-07"
  - file: 15-usecase-urs-srs-commands.md
    title: "UseCase, URS and SRS Commands"
    status: done
    date: "2026-04-07"
  - file: 16-agent-attempt-commands.md
    title: "AgentSlot, AgentInstance and TaskAttempt Commands"
    status: done
    date: "2026-04-07"
  - file: 17-remaining-api-endpoints.md
    title: "Remaining API Endpoints"
    status: done
    date: "2026-04-07"
  - file: 18-domain-event-dispatcher.md
    title: "Domain Event Dispatcher"
    status: done
    date: "2026-04-07"
  - file: 19-llm-provider-groq.md
    title: "ILlmProvider Abstraction and Groq Client"
    status: done
    date: "2026-04-07"
  - file: 20-llm-provider-llamasharp.md
    title: "LlamaSharp Local LLM Provider"
    status: done
    date: "2026-04-07"
  - file: 21-agent-worker-base.md
    title: "Agent Worker Base Infrastructure"
    status: done
    date: "2026-04-12"
  - file: 22-agent-business-analyst.md
    title: "BusinessAnalyst Agent Worker"
    status: done
    date: "2026-04-12"
  - file: 23-agent-architect.md
    title: "Architect Agent Worker"
    status: done
    date: "2026-04-12"
  - file: 24-agent-scrum-master.md
    title: "ScrumMaster Agent Worker"
    status: done
    date: "2026-04-12"
  - file: 25-agent-development-cycle.md
    title: "Development Cycle Agent Workers"
    status: done
    date: "2026-04-12"
  - file: 26-orchestrator-phase.md
    title: "Phase Orchestrator — Domain Event Notification Handlers"
    status: ready
    date: "2026-04-12"
  - file: 27-orchestrator-dev-loop.md
    title: "Development Loop Orchestration Handlers"
    status: ready
    date: "2026-04-12"
  - file: 28-butterfly-service.md
    title: "ButterflyService — EditAndAccept Downstream Propagation"
    status: ready
    date: "2026-04-12"
  - file: 29-signalr-hub.md
    title: "SignalR Hub and Infrastructure"
    status: ready
    date: "2026-04-12"
  - file: 30-signalr-notification-handlers.md
    title: "SignalR Notification Handlers — Domain Events to Frontend"
    status: ready
    date: "2026-04-12"
  - file: 31-agent-instance-activation.md
    title: "Agent Instance Activation Handler"
    status: ready
    date: "2026-04-12"
---

# Feature Specifications

Feature specs define the scope, domain impact, and test expectations for a feature before implementation begins.
They are written by a reasoning partner (human or planning AI) and consumed by the implementation contributor.

Specs are **not standing rules**. They are point-in-time documents that describe intent for a specific feature.
Once a feature is fully implemented and merged, its spec is kept for historical reference but is no longer authoritative.

## Status Vocabulary
- `draft` — being written, not ready for implementation
- `ready` — approved and ready for implementation
- `in-progress` — implementation has started
- `done` — feature is implemented and merged

## Documents

| Title | Status | Date |
|-------|--------|------|
| Domain Entities | done | 2026-03-29 |
| Domain Unit Tests | done | 2026-04-03 |
| Repository Interfaces | done | 2026-04-03 |
| Domain Events | done | 2026-04-03 |
| Application Pipeline | done | 2026-04-03 |
| Project Commands | done | 2026-04-03 |
| WorkTask Commands | done | 2026-04-03 |
| RevisionGate Commands | done | 2026-04-03 |
| EF Core Configuration | done | 2026-04-03 |
| Repository Implementations | done | 2026-04-03 |
| API Wiring and Project Endpoints | done | 2026-04-03 |
| Project Queries | done | 2026-04-07 |
| WorkTask and RevisionGate Queries | done | 2026-04-07 |
| UseCase, URS and SRS Queries | done | 2026-04-07 |
| AgentSlot, AgentInstance and TaskAttempt Queries | done | 2026-04-07 |
| UseCase, URS and SRS Commands | done | 2026-04-07 |
| AgentSlot, AgentInstance and TaskAttempt Commands | done | 2026-04-07 |
| Remaining API Endpoints | done | 2026-04-07 |
| Domain Event Dispatcher | done | 2026-04-07 |
| ILlmProvider Abstraction and Groq Client | done | 2026-04-07 |
| LlamaSharp Local LLM Provider | done | 2026-04-07 |
| Agent Worker Base Infrastructure | done | 2026-04-12 |
| BusinessAnalyst Agent Worker | done | 2026-04-12 |
| Architect Agent Worker | done | 2026-04-12 |
| ScrumMaster Agent Worker | done | 2026-04-12 |
| Development Cycle Agent Workers | done | 2026-04-12 |
| Phase Orchestrator — Domain Event Notification Handlers | ready | 2026-04-12 |
| Development Loop Orchestration Handlers | ready | 2026-04-12 |
| ButterflyService — EditAndAccept Downstream Propagation | ready | 2026-04-12 |
| SignalR Hub and Infrastructure | ready | 2026-04-12 |
| SignalR Notification Handlers — Domain Events to Frontend | ready | 2026-04-12 |
| Agent Instance Activation Handler | ready | 2026-04-12 |

---

## Adding a New Spec
Copy `_template.md` to a new file named after the feature (e.g. `user-authentication.md`).
Fill in all sections completely before marking the spec as `ready`.
Add the new file to the `documents` table and frontmatter list in this README.

> A spec marked `ready` must have no empty sections and no unresolved open questions.
