---
category: toolchain
title: Configuration
covers: ["configuration", "appsettings", "environment variables", "ConnectionStrings", "Groq", "LlamaSharp", "CORS", "AllowedOrigins", "PollingIntervalMs"]
---

# Configuration

All configuration keys, their types, requirements, and defaults.

## Key Reference

| Key | Type | Required | Default | Notes |
|-----|------|----------|---------|-------|
| `ConnectionStrings:DefaultConnection` | string | **Yes** | — | SQLite connection string. App throws on startup if missing. |
| `Groq:ApiKey` | string | No | `""` | Empty = LLM calls fail at runtime with a descriptive error |
| `Groq:Model` | string | No | `llama-3.3-70b-versatile` | Groq model ID |
| `LlamaSharp:ModelPath` | string | No | `""` | Empty or file missing = local inference disabled; `DisabledLlmProvider` registered |
| `LlamaSharp:MaxTokens` | int | No | `512` | Max output tokens for local inference |
| `Agents:PollingIntervalMs` | int | No | `3000` | Agent poll interval in milliseconds |
| `Cors:AllowedOrigins` | string[] | No | `[]` | Empty = allow all origins (dev-friendly). Set in production. |

## Environment variable mapping

.NET maps `__` to `:` in environment variables. Examples:

```
ConnectionStrings__DefaultConnection=Data Source=/data/chaosforge.db
Groq__ApiKey=gsk_...
LlamaSharp__ModelPath=/models/llama3.gguf
Cors__AllowedOrigins__0=https://myapp.example.com
```

## Startup behaviour

- Missing `ConnectionStrings:DefaultConnection` → `InvalidOperationException` at startup with a descriptive message.
- Missing or non-existent `LlamaSharp:ModelPath` → `DisabledLlmProvider` registered; startup warning logged; app continues.
- Missing `Groq:ApiKey` → app starts; agent LLM calls fail at runtime with a Groq 401 error.

## Production settings

`appsettings.Production.json` sets `Warning` as the default log level and leaves
`Cors:AllowedOrigins` empty. Override `AllowedOrigins` via environment variable for
production deployments.
