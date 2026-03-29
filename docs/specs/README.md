---
category: specs
last_updated: "YYYY-MM-DD"
documents:
  - file: example-feature.md
    title: "Example Feature"
    status: draft
    date: "YYYY-MM-DD"
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
| Example Feature | draft | YYYY-MM-DD |

---

## Adding a New Spec
Copy `_template.md` to a new file named after the feature (e.g. `user-authentication.md`).
Fill in all sections completely before marking the spec as `ready`.
Add the new file to the `documents` table and frontmatter list in this README.

> A spec marked `ready` must have no empty sections and no unresolved open questions.
