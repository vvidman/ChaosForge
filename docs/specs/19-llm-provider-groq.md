---
category: specs
title: "ILlmProvider Abstraction and Groq Client"
branch: "llm-groq"
status: ready
date: "2026-04-07"
related_domain: []
related_adr: [004-illmprovider-abstraction, 007-llamasharp-vs-ollama]
---

# Feature Spec — ILlmProvider Abstraction and Groq Client

<!-- Reference this file in the implementation agent with: implement @docs/specs/19-llm-provider-groq.md -->

---

## Context

The agent pipeline needs to call an LLM to generate outputs. Before any agent can be
implemented, the LLM abstraction layer must exist: a provider-agnostic interface
(`ILlmProvider`) in the Application layer, and the first concrete implementation using
the Groq API (cloud, free tier, Llama 3.3 70B) in Infrastructure. LlamaSharp (local CPU)
comes in spec 20. This spec establishes the contract and the primary provider.
ADR-004 and ADR-007 define the reasoning behind this split.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: `ILlmProvider` (Application layer)

---

## Architecture Decisions

- `ILlmProvider` lives in `Application/Abstractions/ILlmProvider.cs`. It belongs in
  Application — not Domain — because it is an infrastructure concern accessed by Application
  use cases (agent handlers). Domain has no knowledge of LLMs.
- Interface contract:
  ```csharp
  public interface ILlmProvider
  {
      /// <summary>
      /// Sends a prompt and returns the LLM's text response.
      /// </summary>
      Task<string> CompleteAsync(
          string systemPrompt,
          string userPrompt,
          CancellationToken cancellationToken = default);
  }
  ```
- No streaming in this spec — `CompleteAsync` returns the full response as a string.
- The Groq implementation uses `HttpClient` — no third-party Groq SDK. Raw REST call to
  `https://api.groq.com/openai/v1/chat/completions` (OpenAI-compatible endpoint).
- Request payload: `{ model, messages: [{role:"system",...},{role:"user",...}], temperature: 0.7 }`.
- Model used: `llama-3.3-70b-versatile` (configurable via `appsettings.json`).
- API key is read from configuration key `"Groq:ApiKey"` — never hardcoded.
- `GroqLlmProvider` is `internal sealed`, registered as a named `HttpClient` via
  `IHttpClientFactory`. Base address: `https://api.groq.com`.
- If the HTTP response is non-success or the response body cannot be parsed, throw a
  descriptive `InvalidOperationException` — do not return null or empty string silently.
- DI registration: `services.AddGroqLlmProvider(configuration)` extension method in
  `Infrastructure/DependencyInjection.cs`. Registers `GroqLlmProvider` as
  `ILlmProvider` with Scoped lifetime.
- Configuration section in `appsettings.json`:
  ```json
  "Groq": {
    "ApiKey": "",
    "Model": "llama-3.3-70b-versatile"
  }
  ```
  The actual API key must be set via user secrets or environment variable — never committed.

---

## Implementation Scope — What must be done

- [ ] Create `Application/Abstractions/ILlmProvider.cs` with the interface above
- [ ] Create `Infrastructure/LLM/GroqLlmProvider.cs` (`internal sealed`):
  - Constructor injects `HttpClient` (named) and `IOptions<GroqOptions>`
  - `CompleteAsync` builds the JSON payload, POSTs to `/openai/v1/chat/completions`,
    deserializes `choices[0].message.content`, and returns it
  - Throws `InvalidOperationException` on HTTP error or missing content
- [ ] Create `Infrastructure/LLM/GroqOptions.cs`:
  ```csharp
  public sealed class GroqOptions
  {
      public string ApiKey { get; init; } = string.Empty;
      public string Model { get; init; } = "llama-3.3-70b-versatile";
  }
  ```
- [ ] Register in `DependencyInjection.cs`:
  - `services.Configure<GroqOptions>(configuration.GetSection("Groq"))`
  - Named `HttpClient` with base address and `Authorization: Bearer {ApiKey}` header
  - `services.AddScoped<ILlmProvider, GroqLlmProvider>()`
- [ ] Add Groq config section to `appsettings.json` (ApiKey empty, Model set)
- [ ] Add `appsettings.Development.json` entry with placeholder key comment
- [ ] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement LlamaSharp in this spec — that is spec 20
- Do not implement any agent logic — that is a future spec
- Do not add retry or circuit breaker logic — future concern
- Do not add streaming support

---

## Test Expectations

- Unit tests required for:
  - `GroqLlmProvider.CompleteAsync`: mock `HttpClient` returning valid response → correct
    string returned; mock returning non-success status → `InvalidOperationException` thrown
- Edge cases to cover: response with missing `choices` array → throws, not returns empty

---

## Open Questions

- None.
