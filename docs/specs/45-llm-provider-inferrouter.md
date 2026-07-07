---
category: specs
title: "InferRouter LLM Provider"
branch: "llm-router"
status: done
date: "2026-07-07"
related_domain: []
related_adr: [011-inferrouter-integration]
---

# Feature Spec — InferRouter LLM Provider

<!-- Reference this file in the implementation agent with: implement @docs/specs/45-llm-provider-inferrouter.md -->

---

## Context

Specs 19 and 20 implemented `ILlmProvider` via two direct SDK integrations: `GroqLlmProvider` (HTTP, cloud) and `LlamaSharpLlmProvider` (in-process, local). ADR-011 replaces both with a single `InferRouterLlmProvider` that calls the companion InferRouter service's OpenAI-compatible `/v1/chat/completions` endpoint, using InferRouter's `PreferredProviderName` extension (InferRouter ADR-011) to preserve the existing role-based routing preference. Depends on: spec 19 (`ILlmProvider` interface, unchanged), spec 20 (`ILlmProviderSelector`, keys change).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none — `ILlmProvider` and `ILlmProviderSelector` contracts are unchanged

---

## Architecture Decisions

- Full design rationale and code sketch: `docs/adr/011-inferrouter-integration.md` — follow it, do not re-derive the design.
- `InferRouterLlmProvider` is `internal sealed`, lives in `Infrastructure/LLM/`, replaces both `GroqLlmProvider` and `LlamaSharpLlmProvider`.
- Constructor takes `HttpClient` and a `string preferredProviderName` — never expose provider names above Infrastructure.
- Registered as a named `HttpClient` via `AddHttpClient<InferRouterLlmProvider>`, base address from `InferRouterOptions.BaseUrl` (config section `"InferRouter"`).
- Two keyed `ILlmProvider` registrations, both backed by `InferRouterLlmProvider` with different `preferredProviderName`:
  - `"cloud-preferred"` → `preferredProviderName: "groq"`
  - `"local-preferred"` → `preferredProviderName: "local-llama"`
- `LlmProviderSelector` role mapping is unchanged in intent, only the resolved keys change:
  - `"cloud-preferred"`: `BusinessAnalyst`, `Architect`, `ScrumMaster`
  - `"local-preferred"`: `Developer`, `Tester`, `Reviewer`, `TechnicalWriter`
- Confirmed contract, endpoint `POST {InferRouter:BaseUrl}/v1/chat/completions` (plain HTTP, not HTTPS — no certificate handling needed). ChaosForge defines its own DTOs mirroring InferRouter's wire format, scoped to only the fields it actually sends/reads (YAGNI/ISP — no `Stream`, `TopP`, penalties, `Tools`, or `ToolChoice`, since none are populated in this spec):

  ```csharp
  internal sealed record OpenAiChatRequest(
      [property: JsonPropertyName("model")] string Model,
      [property: JsonPropertyName("messages")] List<OpenAiMessage> Messages,
      [property: JsonPropertyName("preferred_provider_name")] string? PreferredProviderName = null);

  internal sealed record OpenAiMessage(
      [property: JsonPropertyName("role")] string Role,
      [property: JsonPropertyName("content")] string? Content);

  internal sealed record OpenAiChatResponse(
      [property: JsonPropertyName("id")] string Id,
      [property: JsonPropertyName("object")] string Object,
      [property: JsonPropertyName("created")] long Created,
      [property: JsonPropertyName("model")] string Model,
      [property: JsonPropertyName("system_fingerprint")] string? SystemFingerprint,
      [property: JsonPropertyName("choices")] List<OpenAiChoice> Choices,
      [property: JsonPropertyName("usage")] OpenAiUsage? Usage);

  internal sealed record OpenAiChoice(
      [property: JsonPropertyName("index")] int Index,
      [property: JsonPropertyName("message")] OpenAiMessage Message,
      [property: JsonPropertyName("finish_reason")] string? FinishReason);

  internal sealed record OpenAiUsage(
      [property: JsonPropertyName("prompt_tokens")] int PromptTokens,
      [property: JsonPropertyName("completion_tokens")] int CompletionTokens,
      [property: JsonPropertyName("total_tokens")] int TotalTokens);
  ```

  `Model` is non-nullable on the wire contract but InferRouter does not use it for routing (routing is driven by `PreferredProviderName`); `InferRouterLlmProvider` sends `Model = string.Empty`. `Usage` is deserialized but not consumed yet — kept for future cost/token logging, not exposed through `ILlmProvider` in this spec.
- Response parsing: `response.Choices[0].Message.Content`. Missing/empty `Choices` or null `Content` throws `InvalidOperationException` — same fail-fast convention as `GroqLlmProvider` (spec 19), never return null or empty string silently. Non-success HTTP status also throws `InvalidOperationException` with the status code in the message.
- `GroqOptions` and `LlamaSharpOptions` (and their `appsettings.json` sections) are removed, replaced by `InferRouterOptions { BaseUrl }`.
- InferRouter runs on a separate host on the local network, not on `localhost` — the real address must not be committed to the public example config. `appsettings.Development.example.json` gets a placeholder: `"InferRouter:BaseUrl": "http://<your-inferrouter-host>:5100"`. The real reachable address goes only into the git-ignored `appsettings.Development.json`, consistent with how the previous `Groq` API key was kept out of the example file.

---

## Implementation Scope — What must be done

- [x] Remove `GroqLlmProvider`, `GroqOptions`, and their DI registration
- [x] Remove `LlamaSharpLlmProvider`, `LlamaSharpOptions`, and their DI registration (and the `LLamaSharp` NuGet package reference if nothing else in the project uses it)
- [x] Add `InferRouterOptions` and bind it from the `"InferRouter"` config section
- [x] Implement `InferRouterLlmProvider : ILlmProvider` calling `/v1/chat/completions`, using the `OpenAiChatRequest`/`OpenAiChatResponse` DTOs defined above
- [x] Register `InferRouterLlmProvider` as a typed `HttpClient`, and expose it via two keyed `ILlmProvider` registrations (`"cloud-preferred"`, `"local-preferred"`)
- [x] Update `LlmProviderSelector` to resolve the new keys; role-to-provider mapping itself does not change
- [x] Update `appsettings.Development.example.json`: remove `Groq` and `LlamaSharp` sections, add `InferRouter: { "BaseUrl": "http://<your-inferrouter-host>:5100" }` (placeholder, not a real address); set the real address only in the git-ignored `appsettings.Development.json`
- [x] Delete `GroqLlmProviderTests.cs` and the LlamaSharp provider test file; add `InferRouterLlmProviderTests.cs` (see Test Expectations)
- [x] Update `LlmProviderSelector` tests to assert the new keyed resolution
- [x] Update `docs/architecture/llm-strategy.md` to describe the InferRouter-backed design instead of the Groq/LlamaSharp split
- [x] Update the root `README.md`: architecture Mermaid diagram (single `InferRouter` node instead of separate `Groq`/`Llama` nodes) and "Getting Started" prerequisites (InferRouter running and reachable, instead of a Groq API key and a local GGUF file)
- [x] Set `docs/specs/19-llm-provider-groq.md` and `docs/specs/20-llm-provider-llamasharp.md` frontmatter `status: superseded`, add `related_adr: [011-inferrouter-integration]`, and add a one-line note at the top of each pointing to this spec and to ADR-011
- [x] Update `docs/specs/README.md`: add this spec's row/frontmatter entry (status `ready`, to be flipped to `done` per the normal workflow), and update the status column for specs 19 and 20 to `superseded`
- [x] Update `docs/adr/README.md`: add the ADR-011 frontmatter entry and table row

## Out of Scope — What must NOT be done

- Streaming, tool calling, or extended sampling parameters (`top_p`, penalties) — not adopted in this spec even though InferRouter supports them
- Any change to the InferRouter repository itself — this spec assumes InferRouter ADR-011 (`PreferredProviderName`) is already implemented and deployed there
- Docker Compose orchestration of ChaosForge and InferRouter together — InferRouter is started separately, per existing decision
- Any Microsoft Agent Framework (MAF) related work

---

## Test Expectations

- Unit tests required for:
  - `InferRouterLlmProvider.CompleteAsync` — success path returns parsed content
  - Non-success HTTP status → throws `InvalidOperationException` referencing the status code
  - Empty response body / missing `choices` / null `content` → throws `InvalidOperationException`, never returns null or empty silently
  - Outgoing request body contains the expected `preferred_provider_name` for each keyed instance (mirror the fake-`HttpMessageHandler` pattern already used in `GroqLlmProviderTests`)
  - `LlmProviderSelector` resolves `"cloud-preferred"` for `BusinessAnalyst`/`Architect`/`ScrumMaster` and `"local-preferred"` for the remaining four roles
- Edge cases to cover:
  - InferRouter unreachable (`HttpRequestException`) surfaces as a clear, non-swallowed failure
  - Cancellation token is honored (existing convention from `GroqLlmProvider`)

---

## Open Questions

None. The wire contract (`OpenAiChatRequest`/`OpenAiChatResponse`) and endpoint protocol were confirmed directly against the InferRouter implementation before this spec was marked `ready`.
