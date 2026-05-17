---
category: specs
title: "Configuration Management and Production Readiness"
branch: "config-prod"
status: ready
date: "2026-04-25"
related_domain: []
related_adr: []
---

# Feature Spec — Configuration Management and Production Readiness

<!-- Reference this file in the implementation agent with: implement @docs/specs/44-config-prod.md -->

---

## Context

The application currently fails at startup if `LlamaSharp:ModelPath` is not set, making
it impossible to run without a local model file even on machines where only Groq is needed.
This spec fixes that with graceful degradation, adds startup validation for the one truly
required config value (`ConnectionStrings:DefaultConnection`), tightens CORS for production,
and documents the full configuration surface. Depends on: spec 43 (Docker).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

### LlamaSharp graceful degradation

The current `LlamaSharpLlmProvider` constructor throws `InvalidOperationException` when
`ModelPath` is empty. Because `AddSingleton<LlamaSharpLlmProvider>()` defers construction
to first resolve, the error surfaces at first agent poll cycle, not at startup — which is
confusing and hard to diagnose.

**Fix:** move the `ModelPath` check out of the constructor and into the DI registration
method (`AddLlamaSharpLlmProvider`). If `ModelPath` is null, empty, or points to a
non-existent file, register `DisabledLlmProvider` as the keyed `"llama"` service instead
of `LlamaSharpLlmProvider`. Log a startup warning. `LlamaSharpLlmProvider` itself should
NOT be registered when disabled — it must not be constructed at all.

```csharp
private static IServiceCollection AddLlamaSharpLlmProvider(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.Configure<LlamaSharpOptions>(configuration.GetSection("LlamaSharp"));

    var modelPath = configuration["LlamaSharp:ModelPath"];
    var modelExists = !string.IsNullOrWhiteSpace(modelPath) && File.Exists(modelPath);

    if (!modelExists)
    {
        // Register a no-op provider — LlamaSharpLlmProvider is never constructed
        services.AddKeyedSingleton<ILlmProvider>("llama", (_, _) => new DisabledLlmProvider());
        // Warning is logged via ILogger but DI doesn't have ILogger here; use a startup
        // filter or just Console.WriteLine for simplicity at this stage
        Console.WriteLine("[WARN] LlamaSharp model not found — local inference disabled.");
    }
    else
    {
        services.AddSingleton<LlamaSharpLlmProvider>();
        services.AddKeyedSingleton<ILlmProvider>("llama",
            (sp, _) => sp.GetRequiredService<LlamaSharpLlmProvider>());
    }

    services.AddScoped<ILlmProviderSelector, LlmProviderSelector>();
    return services;
}
```

### ConnectionString startup validation

`ConnectionStrings:DefaultConnection` is read directly by EF Core — it is not bound to
`IOptions<T>`, so `ValidateOnStart()` cannot be used for it. Instead, add an explicit
guard at the top of `Program.cs`, before `builder.Build()`:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required. " +
        "Set it via appsettings.json or the ConnectionStrings__DefaultConnection environment variable.");
```

This gives a clear, immediate error on startup rather than a cryptic EF Core exception later.

### CORS for production

The current `AllowAnyOrigin` is acceptable for local development but should be
configurable for production. Read `Cors:AllowedOrigins` from configuration:
- If the list is empty or missing: fall back to `AllowAnyOrigin` (dev-friendly default)
- If the list has entries: use `WithOrigins(...)` for those specific origins only

```csharp
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? [];

app.UseCors(policy =>
{
    if (allowedOrigins.Length > 0)
        policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod();
    else
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
});
```

### Production logging

`appsettings.Production.json` sets `Warning` as the default minimum log level to reduce
noise. The `ChaosForge` namespace keeps `Information` so application-level logs remain
visible.

---

## Implementation Scope — What must be done

- [ ] Create `Infrastructure/LLM/DisabledLlmProvider.cs`:
  ```csharp
  internal sealed class DisabledLlmProvider : ILlmProvider
  {
      public Task<string> CompleteAsync(
          string systemPrompt,
          string userPrompt,
          CancellationToken cancellationToken = default)
          => throw new InvalidOperationException(
              "LlamaSharp is not configured. Set LlamaSharp:ModelPath to a valid GGUF model file.");
  }
  ```

- [ ] Update `Infrastructure/DependencyInjection.AddLlamaSharpLlmProvider`:
  - Check `LlamaSharp:ModelPath` before registering anything (see code above)
  - If path is missing or file does not exist: register `DisabledLlmProvider` keyed as
    `"llama"`, do NOT register `LlamaSharpLlmProvider`
  - Log startup warning via `Console.WriteLine` (acceptable for a hobby project)
  - If path is valid: register `LlamaSharpLlmProvider` as before

- [ ] Remove the `ModelPath` null/empty guard from `LlamaSharpLlmProvider` constructor
  — it is no longer the right place for this check

- [ ] Add connection string guard to `Program.cs` (see code above), placed immediately
  after `builder.Services.AddInfrastructure(builder.Configuration)`

- [ ] Update `Program.cs` CORS configuration to read `Cors:AllowedOrigins` (see code above)

- [ ] Create `src/ChaosForge.API/appsettings.Production.json`:
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

- [ ] Create `docs/toolchain/configuration.md` with a table of all config keys:

  | Key | Type | Required | Default | Notes |
  |---|---|---|---|---|
  | `ConnectionStrings:DefaultConnection` | string | **Yes** | — | SQLite connection string |
  | `Groq:ApiKey` | string | No | `""` | Empty = LLM calls fail at runtime |
  | `Groq:Model` | string | No | `llama-3.3-70b-versatile` | Groq model ID |
  | `LlamaSharp:ModelPath` | string | No | `""` | Empty/missing = local inference disabled |
  | `LlamaSharp:MaxTokens` | int | No | `512` | Max output tokens for local inference |
  | `Agents:PollingIntervalMs` | int | No | `3000` | Agent poll interval in ms |
  | `Cors:AllowedOrigins` | string[] | No | `[]` | Empty = allow all origins |

- [ ] `dotnet build` — zero warnings, zero errors
- [ ] Verify: app starts with empty `LlamaSharp:ModelPath` — logs warning, continues
- [ ] Verify: app fails to start with missing `ConnectionStrings:DefaultConnection` —
  shows the descriptive error message

---

## Out of Scope — What must NOT be done

- Do not add a secrets manager (Vault, Azure Key Vault)
- Do not add HTTPS enforcement in code — handled by reverse proxy
- Do not add database migration guards — `MigrateAsync` already runs on startup
- Do not change `LlamaSharpLlmProvider` logic beyond removing the constructor guard

---

## Test Expectations

- Unit tests required for:
  - `DisabledLlmProvider.CompleteAsync` throws `InvalidOperationException` with the
    correct message
- Edge cases to cover:
  - `LlamaSharp:ModelPath` set but file does not exist → `DisabledLlmProvider` registered,
    warning logged, `LlamaSharpLlmProvider` NOT constructed
  - `LlamaSharp:ModelPath` empty → same as above

---

## Open Questions

- None.
