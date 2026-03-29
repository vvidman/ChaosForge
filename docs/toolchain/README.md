---
category: toolchain
last_updated: "2026-03-29"
documents:
  - file: llamasharp.md
    covers: ["LlamaSharp", "GGUF", "local model", "CPU inference", "model path", "context size", "llama.cpp"]
  - file: groq.md
    covers: ["Groq", "API key", "cloud LLM", "rate limit", "llama-3.3-70b", "quota", "429"]
  - file: ef-migrations.md
    covers: ["EF Core", "migrations", "dotnet ef", "schema", "database update", "migration add", "IDesignTimeDbContextFactory", "SQLite"]
---

# Toolchain

Setup and operational guides for the tools that require non-trivial configuration.
These documents describe *how to get things working* — not coding rules or design decisions.

**Note:** Short-form build and run commands live in `docs/conventions/toolchain.md`.
Load documents here when setting up from scratch, troubleshooting, or changing provider configuration.

## Documents

### `llamasharp.md`
Local GGUF model download, path configuration, context size tuning, and startup troubleshooting.
Load when: setting up local inference for the first time, switching models, or diagnosing LlamaSharp errors.

### `groq.md`
Groq API key setup, model selection, free tier rate limits, and quota troubleshooting.
Load when: setting up Groq for the first time, switching models, or hitting rate limit errors.

### `ef-migrations.md`
Full migration workflow: add, apply, revert, and script generation. Naming conventions,
design-time factory setup, and test database initialization.
Load when: adding a new entity or field, reverting a migration, or debugging migration errors.

---

## Adding a New Toolchain Document
Copy `_template.md` to a new file named after the tool or concern (e.g. `signalr-local.md`).
Add the new file to the `documents` list in this README's frontmatter.
