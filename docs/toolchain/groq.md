---
category: toolchain
tool: "Groq — Cloud LLM API"
last_updated: "2026-03-29"
---

# Groq — Cloud LLM API

## Purpose
Groq provides fast cloud inference for complex agent roles (BusinessAnalyst, Architect,
ScrumMaster) where reasoning quality matters more than latency or cost. The free tier
is sufficient for hobby/development use. See `llm-strategy.md` (architecture) for the
role-to-provider mapping.

## Prerequisites
- A Groq account: https://console.groq.com
- Free tier is sufficient — no payment required for development

## Setup

### 1. Create an API key
Log in to https://console.groq.com → API Keys → Create API Key.
Copy the key immediately — it is only shown once.

### 2. Store the key

```bash
dotnet user-secrets set "Groq:ApiKey" "<your-key-here>" \
  --project src/ChaosForge.API
```

Never put the key in `appsettings.json` or any committed file.

### 3. Configure the model

Default configuration in `appsettings.json` (no secrets here):

```json
{
  "Groq": {
    "Model": "llama-3.3-70b-versatile",
    "MaxTokens": 4096
  }
}
```

### 4. Verify

```bash
dotnet run --project src/ChaosForge.API
```

`GroqProvider` validates the API key at startup with a lightweight health check.
A missing or invalid key throws at DI registration time.

## Common Tasks

### Switching to a different Groq model
Update `Groq:Model` in `appsettings.json`. Available models:

| Model | Use case |
|---|---|
| `llama-3.3-70b-versatile` | Default — best reasoning, recommended for BA/Architect/SM |
| `llama-3.1-8b-instant` | Faster, lower quality — use for testing/dev iteration |
| `gemma2-9b-it` | Alternative if Llama quota is exhausted |

### Checking remaining quota
Free tier limits reset daily. Check at: https://console.groq.com/settings/limits

## Troubleshooting

### `401 Unauthorized`
**Cause:** API key missing, expired, or incorrectly stored.
**Fix:** Re-run `dotnet user-secrets set "Groq:ApiKey" "..."` and verify with `dotnet user-secrets list --project src/ChaosForge.API`.

### `429 Too Many Requests`
**Cause:** Free tier rate limit hit. Limits are per-minute and per-day per model.
**Fix:** Switch to `llama-3.1-8b-instant` temporarily, or wait for the rate limit window to reset (check the `Retry-After` header in the error response). For sustained development, slow down agent dispatch intervals in `appsettings.Development.json`.

### Slow responses (>30s)
**Cause:** Groq free tier occasionally has high latency during peak hours.
**Fix:** Retry is handled automatically by `GroqProvider`. If latency is consistently high, switch to `llama-3.1-8b-instant` for the session.

## References
- See ADR-004 for `ILLMProvider` abstraction rationale
- See `llamasharp.md` for the local provider used by repetitive agent roles
- See `llm-strategy.md` (architecture) for role-to-provider mapping
