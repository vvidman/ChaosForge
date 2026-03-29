---
category: specs
title: "[Feature Name]"
branch: "[unambiguous branch name generated from the feature name, max 12 character]"
status: draft
date: "YYYY-MM-DD"
related_domain: []
related_adr: []
---

# Feature Spec — [Feature Name]

<!-- INSTRUCTIONS:
     Write this file in your reasoning partner session (planning AI or human).
     The implementation contributor reads it and implements from it — do not leave ambiguities.
     Reference this file in the implementation agent with: implement @docs/specs/this-file.md

     Replace every [placeholder] and remove these comment blocks before marking status as ready. -->

---

## Context

<!-- Why is this feature needed? Which part of the system does it affect?
     What would break or be missing without it? 2-4 sentences. -->
TODO

---

## Domain Impact

<!-- What changes in the domain model? Be explicit — the implementor will use this
     to decide what to create. -->

- New or modified entity: _none / [EntityName with field changes]_
- New domain event: _none / [EventName]_
- New interface: _none / [IInterfaceName]_

---

## Architecture Decisions

<!-- Decisions made during planning that the implementor must follow without deviation.
     If there are trade-offs, explain them here so they are not re-litigated during implementation.

     Example:
     - Use a BackgroundService for polling, not a timer-based approach
     - The domain event must be dispatched before the database write is committed -->
TODO

---

## Implementation Scope — What must be done

<!-- A checklist. Each item should be a concrete, verifiable task. -->

- [ ] TODO
- [ ] TODO
- [ ] TODO

## Out of Scope — What must NOT be done

<!-- Explicit exclusions prevent scope creep. List anything adjacent that might seem
     related but must not be touched in this feature. -->

- TODO
- TODO

---

## Test Expectations

<!-- What must be tested? What edge cases are non-negotiable? -->

- Unit tests required for:
- Edge cases to cover:

---

## Open Questions

<!-- Anything unresolved before implementation starts.
     The implementor should surface these in review if they remain unanswered. -->

- TODO
