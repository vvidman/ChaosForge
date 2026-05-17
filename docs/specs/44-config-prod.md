---
category: specs
title: "Configuration Management and Production Readiness"
branch: "config-prod"
status: ready
date: "2026-04-21"
related_domain: []
related_adr: []
---

# Feature Spec — Configuration Management and Production Readiness

<!-- Reference this file in the implementation agent with: implement @docs/specs/44-config-prod.md -->

---

## Context

The application currently relies on hardcoded values in `appsettings.json` and developer
user secrets. This spec makes all sensitive and environment-specific configuration
injectable via environment variables, documents the configuration surface, and adds a
startup validation step that fails fast if required config is missing. For a hobby
project, this is the appropriate level of "production readiness" — it can run on a VPS
or home server with only environment variables, no manual file editing. Depends on:
spec 43 (Docker).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- All secrets and environment-specific values are configurable via environment variables
  using .NET's double-underscore (`__`) section separator convention.
- **Startup validation:** add an `IStartupFilter` or use `IOptions<T>` with
  `[Required]` data annotations + `ValidateOnStart()` to fail fast if required config
  is absent or invalid.
- Required config (app fails to start if missing/invalid):
  - `ConnectionStrings:DefaultConnection` — must not be empty
- Optional config (app starts, feature degrades gracefully):
  - `Groq:ApiKey` — empty string is allowed; LLM calls will fail at runtime
  - `LlamaSharp:ModelPath` — empty string means LlamaSharp is disabled
- **LlamaSharp graceful degradation:** if `ModelPath` is empty or file does not exist,
  do NOT throw at startup — instead, register a `DisabledLlmProvider` that throws a
  descriptive `InvalidOperationException` at call time with message:
  "LlamaSharp is not configured. Set LlamaSharp:ModelPath to a valid GGUF model file."
  This replaces the current fail-fast behaviour in `LlamaSharpLlmProvider`.
- **Logging:** configure structured logging with `appsettings.Production.json` that
  sets minimum level to `Warning` (instead of `Information`) to reduce noise in
  production. Development keeps `Information`.
- **CORS in production:** tighten CORS from `AllowAnyOrigin` to a configurable
  `AllowedOrigins` list. If empty, keep `AllowAnyOrigin` as fallback (dev-friendly).
  Add config key `"Cors:AllowedOrigins": []`.
- **`appsettings.Production.json`:** create with appropriate overrides:
  ```json
  {
    "Logging": {
      "LogLevel": {
        "Default": "Warning",
        "Microsoft.AspNetCore": "Warning",
        "ChaosForge": "Information"
      }
    },
    "Cors": {
      "AllowedOrigins": []
    }
  }
  ```

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/LLM/DisabledLlmProvider.cs`:
  ```csharp
  internal sealed class DisabledLlmProvider : ILlmProvider
  {
      public Task<string> CompleteAsync(string systemPrompt, string userPrompt,
          CancellationToken cancellationToken = default)
          => throw new InvalidOperationException(
              "LlamaSharp is not configured. Set LlamaSharp:ModelPath to a valid GGUF model file.");
  }
  ```
- [ ] Update `DependencyInjection.AddLlamaSharpLlmProvider`:
  - If `ModelPath` is null/empty/non-existent: register `DisabledLlmProvider` as the
    `"llama"` keyed service instead of `LlamaSharpLlmProvider`
  - Log a warning at startup: "LlamaSharp model not configured — local inference disabled"
- [ ] Add `ValidateDataAnnotations()` and `ValidateOnStart()` to
  `ConnectionStrings` options in DI
- [ ] Create `appsettings.Production.json`
- [ ] Update CORS configuration in `Program.cs` to read `Cors:AllowedOrigins` and use
  specific origins if set, `AllowAnyOrigin` if empty
- [ ] Create `docs/toolchain/configuration.md` documenting all configuration keys,
  their types, defaults, and whether they are required
- [ ] Verify: app starts without `LlamaSharp:ModelPath` set (development scenario)
- [ ] Verify: app starts without `Groq:ApiKey` set (only LLM calls fail, not startup)
- [ ] `dotnet build` — zero warnings

---

## Out of Scope — What must NOT be done

- Do not add a secrets manager (Vault, Azure Key Vault) — environment variables are
  sufficient for a hobby project
- Do not add HTTPS enforcement in code — handled by reverse proxy in production
- Do not add database migration guards at startup — `MigrateAsync` already runs

---

## Test Expectations

- Unit tests required for:
  - `DisabledLlmProvider.CompleteAsync` throws `InvalidOperationException` with the
    correct message
- Edge cases to cover: `ModelPath` is set but file does not exist — also registers
  `DisabledLlmProvider` and logs warning (not throws at startup)

---

## Open Questions

- None.
