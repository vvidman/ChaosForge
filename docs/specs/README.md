---
category: specs
last_updated: "2026-05-17"
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
    status: superseded
    date: "2026-04-07"
  - file: 20-llm-provider-llamasharp.md
    title: "LlamaSharp Local LLM Provider"
    status: superseded
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
    status: done
    date: "2026-04-12"
  - file: 27-orchestrator-dev-loop.md
    title: "Development Loop Orchestration Handlers"
    status: done
    date: "2026-04-12"
  - file: 28-butterfly-service.md
    title: "ButterflyService — EditAndAccept Downstream Propagation"
    status: done
    date: "2026-04-12"
  - file: 29-signalr-hub.md
    title: "SignalR Hub and Infrastructure"
    status: done
    date: "2026-04-12"
  - file: 30-signalr-notification-handlers.md
    title: "SignalR Notification Handlers — Domain Events to Frontend"
    status: done
    date: "2026-04-12"
  - file: 31-agent-instance-activation.md
    title: "Agent Instance Activation Handler"
    status: done
    date: "2026-04-12"
  - file: 32-fe-toolchain.md
    title: "Frontend Toolchain and Design System"
    status: done
    date: "2026-04-21"
  - file: 33-fe-api-hooks.md
    title: "API Client Layer and React Query Hooks"
    status: done
    date: "2026-04-21"
  - file: 34-fe-signalr.md
    title: "SignalR Client and Real-Time State"
    status: done
    date: "2026-04-21"
  - file: 35-fe-project-list.md
    title: "Project List and Create Page"
    status: done
    date: "2026-04-21"
  - file: 36-fe-project-detail.md
    title: "Project Detail Shell and Overview Tab"
    status: done
    date: "2026-04-21"
  - file: 37-fe-gate-judge.md
    title: "Revision Gate Judge Interface"
    status: done
    date: "2026-04-21"
  - file: 38-fe-requirements-tab.md
    title: "Requirements Pipeline Tab"
    status: done
    date: "2026-04-21"
  - file: 39-fe-sprint-board.md
    title: "Sprint Board Tab"
    status: done
    date: "2026-04-21"
  - file: 40-fe-agent-monitor.md
    title: "Agent Monitor Tab"
    status: done
    date: "2026-04-21"
  - file: 41-fe-history-tab.md
    title: "Task Attempt History Tab"
    status: done
    date: "2026-04-21"
  - file: 42-fe-ux-polish.md
    title: "Global UX Polish — Loading, Errors, Animations, Accessibility"
    status: done
    date: "2026-05-17"
  - file: cr-fix-01-worktask-by-project-endpoint.md
    title: "CR Fix: WorkTask by-project API endpoint"
    status: done
    date: "2026-04-25"
  - file: cr-fix-02-env-var-naming.md
    title: "CR Fix: Env var naming inconsistency in SignalRContext"
    status: done
    date: "2026-04-25"
  - file: cr-fix-05-misc-stale.md
    title: "CR Fix: RevisionGatePage stale placeholder and hook typo"
    status: done
    date: "2026-04-25"
  - file: cr-fix-03-kanban-grouping.md
    title: "CR Fix: KanbanBoard incorrect grouping logic"
    status: done
    date: "2026-04-25"
  - file: cr-fix-04-status-badge-generic.md
    title: "CR Fix: StatusBadge only handles ProjectStatus"
    status: done
    date: "2026-04-25"
  - file: cr-fix-06-signalr-invalidation.md
    title: "CR Fix: SignalR WorkTaskStatusChanged cache invalidation too broad"
    status: done
    date: "2026-04-25"
  - file: 43-docker.md
    title: "Docker and docker-compose"
    status: done
    date: "2026-04-21"
  - file: 44-config-prod.md
    title: "Configuration Management and Production Readiness"
    status: done
    date: "2026-04-21"
  - file: 45-llm-provider-inferrouter.md
    title: "InferRouter LLM Provider"
    status: done
    date: "2026-07-07"
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
- `superseded` — replaced by a newer spec; kept for historical reference

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
| ILlmProvider Abstraction and Groq Client | superseded | 2026-04-07 |
| LlamaSharp Local LLM Provider | superseded | 2026-04-07 |
| Agent Worker Base Infrastructure | done | 2026-04-12 |
| BusinessAnalyst Agent Worker | done | 2026-04-12 |
| Architect Agent Worker | done | 2026-04-12 |
| ScrumMaster Agent Worker | done | 2026-04-12 |
| Development Cycle Agent Workers | done | 2026-04-12 |
| Phase Orchestrator — Domain Event Notification Handlers | done | 2026-04-12 |
| Development Loop Orchestration Handlers | done | 2026-04-12 |
| ButterflyService — EditAndAccept Downstream Propagation | done | 2026-04-12 |
| SignalR Hub and Infrastructure | done | 2026-04-12 |
| SignalR Notification Handlers — Domain Events to Frontend | done | 2026-04-12 |
| Agent Instance Activation Handler | done | 2026-04-12 |
| Frontend Toolchain and Design System | done | 2026-04-21 |
| API Client Layer and React Query Hooks | done | 2026-04-21 |
| SignalR Client and Real-Time State | done | 2026-04-21 |
| Project List and Create Page | done | 2026-04-21 |
| Project Detail Shell and Overview Tab | done | 2026-04-21 |
| Revision Gate Judge Interface | done | 2026-04-21 |
| Requirements Pipeline Tab | done | 2026-04-21 |
| Sprint Board Tab | done | 2026-04-21 |
| Agent Monitor Tab | done | 2026-04-21 |
| Task Attempt History Tab | done | 2026-04-21 |
| Global UX Polish — Loading, Errors, Animations, Accessibility | done | 2026-05-17 |
| CR Fix: WorkTask by-project API endpoint | done | 2026-04-25 |
| CR Fix: Env var naming inconsistency in SignalRContext | done | 2026-04-25 |
| CR Fix: RevisionGatePage stale placeholder and hook typo | done | 2026-04-25 |
| CR Fix: KanbanBoard incorrect grouping logic | done | 2026-04-25 |
| CR Fix: StatusBadge only handles ProjectStatus | done | 2026-04-25 |
| CR Fix: SignalR WorkTaskStatusChanged cache invalidation too broad | done | 2026-04-25 |
| Docker and docker-compose | done | 2026-04-21 |
| Configuration Management and Production Readiness | done | 2026-04-21 |
| InferRouter LLM Provider | done | 2026-07-07 |

---

## Adding a New Spec
Copy `_template.md` to a new file named after the feature (e.g. `user-authentication.md`).
Fill in all sections completely before marking the spec as `ready`.
Add the new file to the `documents` table and frontmatter list in this README.

> A spec marked `ready` must have no empty sections and no unresolved open questions.
