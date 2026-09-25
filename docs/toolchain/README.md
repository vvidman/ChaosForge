---
category: toolchain
last_updated: "2026-09-25"
documents:
  - file: llamasharp.md
    status: superseded
    covers: ["LlamaSharp", "GGUF", "local model", "CPU inference", "model path", "context size", "llama.cpp"]
  - file: groq.md
    status: superseded
    covers: ["Groq", "API key", "cloud LLM", "rate limit", "llama-3.3-70b", "quota", "429"]
  - file: ef-migrations.md
    covers: ["EF Core", "migrations", "dotnet ef", "schema", "database update", "migration add", "IDesignTimeDbContextFactory", "SQLite"]
  - file: docker.md
    covers: ["Docker", "docker-compose", "container", "static files", "InferRouter", "host.docker.internal", "production build"]
  - file: configuration.md
    covers: ["configuration", "appsettings", "environment variables", "ConnectionStrings", "InferRouter", "BaseUrl", "CORS", "AllowedOrigins", "PollingIntervalMs"]
---

# Toolchain

Setup and operational guides for the tools that require non-trivial configuration.
These documents describe *how to get things working* — not coding rules or design decisions.

**Note:** Short-form build and run commands live in `docs/conventions/toolchain.md`.
Load documents here when setting up from scratch, troubleshooting, or changing provider configuration.

## Documents

### `llamasharp.md` — superseded by ADR-011
Historical: in-process LlamaSharp inference, used before LLM calls moved to InferRouter.
Do not load for current work.

### `groq.md` — superseded by ADR-011
Historical: direct Groq API integration, used before LLM calls moved to InferRouter.
Do not load for current work.

### `ef-migrations.md`
Full migration workflow: add, apply, revert, and script generation. Naming conventions,
design-time factory setup, and test database initialization.
Load when: adding a new entity or field, reverting a migration, or debugging migration errors.

### `docker.md`
Full-stack Docker setup: build, run, InferRouter URL configuration, and volume management.
Load when: running the app in Docker, setting up CI/CD, or troubleshooting container startup.

### `configuration.md`
Full config key reference table: types, defaults, required/optional, env var mapping, and startup behaviour.
Load when: setting up the app for the first time, debugging a startup error, or configuring CORS for production.

---

## Adding a New Toolchain Document
Copy `_template.md` to a new file named after the tool or concern (e.g. `signalr-local.md`).
Add the new file to the `documents` list in this README's frontmatter.
