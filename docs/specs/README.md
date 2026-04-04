---
category: specs
last_updated: "2026-04-04"
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
    status: ready
    date: "2026-04-03"
  - file: 07-revision-gate-commands.md
    title: "RevisionGate Commands"
    status: ready
    date: "2026-04-03"
  - file: 08-ef-core-configuration.md
    title: "EF Core Configuration"
    status: ready
    date: "2026-04-03"
  - file: 09-repository-implementations.md
    title: "Repository Implementations"
    status: ready
    date: "2026-04-03"
  - file: 10-api-wiring.md
    title: "API Wiring and Project Endpoints"
    status: ready
    date: "2026-04-03"
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
| WorkTask Commands | ready | 2026-04-03 |
| RevisionGate Commands | ready | 2026-04-03 |
| EF Core Configuration | ready | 2026-04-03 |
| Repository Implementations | ready | 2026-04-03 |
| API Wiring and Project Endpoints | ready | 2026-04-03 |

---

## Adding a New Spec
Copy `_template.md` to a new file named after the feature (e.g. `user-authentication.md`).
Fill in all sections completely before marking the spec as `ready`.
Add the new file to the `documents` table and frontmatter list in this README.

> A spec marked `ready` must have no empty sections and no unresolved open questions.
