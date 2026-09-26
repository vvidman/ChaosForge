---
category: toolchain
title: Configuration
covers: ["configuration", "appsettings", "environment variables", "ConnectionStrings", "InferRouter", "BaseUrl", "CORS", "AllowedOrigins", "PollingIntervalMs"]
---

# Configuration

All configuration keys, their types, requirements, and defaults.

## Key Reference

| Key | Type | Required | Default | Notes |
|-----|------|----------|---------|-------|
| `ConnectionStrings:DefaultConnection` | string | **Yes** | — | SQLite connection string. App throws on startup if missing. |
| `InferRouter:BaseUrl` | string | **Yes** | — | Absolute http(s) URL of the InferRouter instance. Validated at startup (ADR-011). |
| `Agents:PollingIntervalMs` | int | No | `3000` | Agent poll interval in milliseconds |
| `Cors:AllowedOrigins` | string[] | No | `[]` | Explicit origins get a credentialed policy (required by SignalR from another origin, e.g. `http://localhost:5173` for the Vite dev server — set in `appsettings.Development.example.json`). Empty = any origin without credentials, suitable only for same-origin hosting (Docker). |

## Environment variable mapping

.NET maps `__` to `:` in environment variables. Examples:

```
ConnectionStrings__DefaultConnection=Data Source=/data/chaosforge.db
InferRouter__BaseUrl=http://host.docker.internal:5100
Cors__AllowedOrigins__0=https://myapp.example.com
```

## Startup behaviour

- Missing `ConnectionStrings:DefaultConnection` → `InvalidOperationException` at startup with a descriptive message.
- Missing, relative or non-http(s) `InferRouter:BaseUrl` → `OptionsValidationException` at startup
  (`InferRouterOptionsValidator`, registered with `ValidateOnStart`).
- Provider API keys and model selection are InferRouter's concern — ChaosForge only sends
  `preferred_provider_name` per role (see `llm-strategy.md`).

## Production settings

`appsettings.Production.json` sets `Warning` as the default log level and leaves
`Cors:AllowedOrigins` empty. Override `AllowedOrigins` via environment variable for
production deployments.
