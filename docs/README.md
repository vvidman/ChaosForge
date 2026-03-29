---
version: "1.0"
last_updated: "YYYY-MM-DD"
categories:
  conventions:
    path: "docs/conventions/README.md"
    triggers: ["code", "style", "lint", "format", "naming", "test", "convention"]
    status: active
  adr:
    path: "docs/adr/README.md"
    triggers: ["architecture", "decision", "pattern", "structure", "why", "approach"]
    status: active
  architecture:
    path: "docs/architecture/README.md"
    triggers: ["principle", "clean architecture", "solid", "design", "layer", "dependency"]
    status: active
  domain:
    path: "docs/domain/README.md"
    triggers: ["feature", "business logic", "entity", "rule", "domain"]
    status: active
  specs:
    path: "docs/specs/README.md"
    triggers: ["spec", "feature spec", "implement", "requirement", "scope"]
    status: active
  toolchain:
    path: "docs/toolchain/README.md"
    triggers: ["build", "deploy", "ci", "cd", "docker", "pipeline", "tooling"]
    status: active
---

# Contributor Knowledge Base

This directory contains the knowledge base for all contributors — human and AI alike.
Start here to identify what knowledge exists and where to find it.
Load only the category and files relevant to your current task.

## Categories

### `conventions/`
Coding standards, naming rules, formatting, and testing patterns.
Load when: writing or reviewing code, onboarding, refactoring.

### `adr/`
Architecture Decision Records. Authoritative source for structural and design decisions.
Load when: proposing changes to architecture, questioning an existing pattern.

### `architecture/`
Design principles and how they are applied in this project (e.g. Clean Architecture, SOLID).
Load when: reasoning about layer boundaries, dependency direction, or design trade-offs.

### `domain/`
Business domain knowledge: entities, rules, and feature area context.
Load when: implementing a feature, reasoning about business logic.

### `specs/`
Feature specifications written before implementation. Define scope, domain impact, and test expectations.
Load when: implementing a specific feature, clarifying what should and should not be built.

### `toolchain/`
Infrastructure, CI/CD pipelines, Docker, and build system how-tos.
Load when: modifying build config, deployment process, or local dev environment setup.

---

## Navigation Rules
1. Read this file first to identify the relevant category
2. Read the category `README.md` to find the specific document
3. Load only what is needed for the current task
4. Never load `_template.md` files — these are authoring scaffolds, not knowledge sources
