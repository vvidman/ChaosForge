---
category: adr
id: "011"
title: "Route LLM calls through InferRouter instead of direct provider SDKs"
status: accepted
date: "2026-07-07"
supersedes: null
superseded_by: null
related_principles: [llm-strategy, solid]
---

# ADR-011: Route LLM calls through InferRouter instead of direct provider SDKs

## Status
`accepted`

## Context
ADR-004 established `ILlmProvider` as the Application-layer abstraction for all LLM calls, implemented by two Infrastructure classes: `GroqLlmProvider` (direct HTTP calls to Groq's OpenAI-compatible endpoint) and `LlamaSharpLlmProvider` (in-process CPU inference). `ILlmProviderSelector` maps each `AgentRole` to one of the two, proactively sending the four high-frequency roles (Developer, Tester, Reviewer, TechnicalWriter) to local inference to protect Groq's free-tier quota, and the three low-frequency, high-complexity roles (BusinessAnalyst, Architect, ScrumMaster) to Groq.

ChaosForge now has a companion project, InferRouter — a self-hosted, OpenAI-compatible LLM routing service that already implements provider fallback, rate-limit tracking, and health checks across Groq, Gemini, and a local GGUF/Ollama final fallback. Maintaining ChaosForge's own Groq and LlamaSharp integrations in parallel with InferRouter duplicates functionality InferRouter already owns, and means every future provider addition (e.g. Anthropic, a second local model) requires a code change in two repositories instead of one config change in InferRouter alone.

ADR-004's "Alternatives Considered" section rejected a generic HTTP-only wrapper (its "Option B") specifically because LlamaSharp's in-process execution avoided HTTP overhead for local CPU inference. That reasoning assumed ChaosForge's own process would host the local model. With InferRouter, local inference still runs in-process — inside InferRouter's process, not ChaosForge's — so the original objection no longer applies to ChaosForge's side of the boundary. This ADR revisits that specific alternative; it does not revisit ADR-004's core decision that `ILlmProvider` is the correct abstraction boundary, which stands unchanged.

Replacing per-role provider selection with a single InferRouter-backed HTTP client would also remove ChaosForge's ability to guarantee that specific roles favor local inference, since InferRouter's own routing strategies select providers reactively by availability and quota, not by caller intent. This was resolved by extending InferRouter itself (see InferRouter ADR-011 — Explicit Provider Targeting) with an optional `PreferredProviderName` on each request, letting ChaosForge keep its proactive role-based preference while still benefiting from InferRouter's shared fallback chain if the preferred provider is unavailable.

## Decision
`GroqLlmProvider` and `LlamaSharpLlmProvider` are removed. A single new Infrastructure class, `InferRouterLlmProvider`, implements `ILlmProvider` by calling InferRouter's `POST /v1/chat/completions` endpoint. The `ILlmProvider` interface contract is unchanged — `CompleteAsync(string systemPrompt, string userPrompt, CancellationToken)` — so Application-layer handlers require no changes.

`InferRouterLlmProvider` accepts a `preferredProviderName` constructor parameter (not a method parameter — see rationale below) and includes it as `preferred_provider_name` on every request it sends.

```csharp
internal sealed class InferRouterLlmProvider(
    HttpClient httpClient,
    string preferredProviderName) : ILlmProvider
{
    public async Task<string> CompleteAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default)
    {
        var request = new
        {
            preferred_provider_name = preferredProviderName,
            messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user", content = userPrompt }
            }
        };

        var response = await httpClient.PostAsJsonAsync(
            "/v1/chat/completions", request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException(
                $"InferRouter request failed: {(int)response.StatusCode} {response.ReasonPhrase}");
        }

        var body = await response.Content
            .ReadFromJsonAsync<ChatCompletionResponse>(cancellationToken)
            ?? throw new InvalidOperationException("InferRouter returned an empty response body.");

        return body.Choices[0].Message.Content
            ?? throw new InvalidOperationException("InferRouter returned no message content.");
    }
}
```

**Why a constructor parameter, not a method parameter:** the Application layer must not know provider names — that would reintroduce the coupling ADR-004 explicitly rejected (its "Option A"). Instead, DI registers two keyed instances of the same class, each closed over its own preferred provider name:

```csharp
private static IServiceCollection AddInferRouterLlmProvider(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.Configure<InferRouterOptions>(configuration.GetSection("InferRouter"));

    services.AddHttpClient<InferRouterLlmProvider>((sp, client) =>
    {
        var options = sp.GetRequiredService<IOptions<InferRouterOptions>>().Value;
        client.BaseAddress = new Uri(options.BaseUrl);
    });

    services.AddKeyedScoped<ILlmProvider>("cloud-preferred", (sp, _) =>
        new InferRouterLlmProvider(
            sp.GetRequiredService<HttpClient>(),
            preferredProviderName: "groq"));

    services.AddKeyedScoped<ILlmProvider>("local-preferred", (sp, _) =>
        new InferRouterLlmProvider(
            sp.GetRequiredService<HttpClient>(),
            preferredProviderName: "local-llama"));

    services.AddScoped<ILlmProviderSelector, LlmProviderSelector>();

    return services;
}
```

`LlmProviderSelector` is unchanged in shape — it still maps `AgentRole` to a keyed `ILlmProvider` — only the two keys it resolves now point at the same concrete class with different constructor state, rather than two different classes.

The `"groq"` and `"local-llama"` values must match the `Name` fields configured in InferRouter's `appsettings.json`. This is an implicit contract between the two repositories and must be documented in both READMEs (see Consequences).

No streaming, tool calling, or extended sampling parameters are adopted in this ADR even though InferRouter supports them — out of scope for the 1.0 milestone, which is a like-for-like provider swap. Adopting them is a candidate for a future ADR once the MAF orchestration work begins.

## Consequences

**Positive:**
- Removes two provider implementations and their tests from ChaosForge; adding a future provider (e.g. Anthropic) becomes an InferRouter-only config change with zero ChaosForge code changes — a stronger Open/Closed story than ADR-004's original two-implementation design.
- ChaosForge gains InferRouter's rate-limit tracking, structured operation logging, and multi-provider fallback (Groq + Gemini before local) for every role — not just the three previously mapped to Groq.
- `ILlmProvider` and `ILlmProviderSelector` contracts are unchanged; Application layer and its tests require no modification.

**Trade-offs:**
- ChaosForge now has a hard runtime dependency on InferRouter being reachable. There is no longer an in-process local fallback if InferRouter itself is down — previously, `LlamaSharpLlmProvider` worked even with zero external services running. This must be called out explicitly in the README's prerequisites, so a demo viewer who starts ChaosForge without InferRouter running gets a clear error, not silent failure.
- Provider name strings (`"groq"`, `"local-llama"`) are duplicated as literals across two repositories with no compile-time check that they match. A typo silently degrades to InferRouter's default routing strategy (per InferRouter ADR-011) rather than failing fast. Acceptable at this project's scope; would need a shared contract at larger scale.
- Specs 19 and 20 (`docs/specs/19-llm-provider-groq.md`, `docs/specs/20-llm-provider-llamasharp.md`) describe the implementation this ADR retires. They are kept for historical reference with `status: superseded` and a pointer to this ADR, per the existing status vocabulary — not deleted.

## Alternatives Considered

### Option A: Keep both paths — direct providers as primary, InferRouter as an additional third option
Rejected on merit: maintaining three provider implementations for two logical backends is worse duplication than the current two, and defeats the purpose of consolidating routing logic into InferRouter.

### Option B: Drop role-based provider selection entirely; let InferRouter's own routing strategy govern all seven roles uniformly
Simpler — `ILlmProviderSelector` and its keyed DI registrations could be deleted outright. Rejected because it silently changes the quota-protection behaviour specs 19/20 were written to guarantee, without a corresponding decision recorded anywhere. If InferRouter's combined Groq+Gemini quota proves sufficient in practice after the 1.0 demo, this option should be revisited — it is the simpler design and should win once proactive control is no longer needed.

### Option C: Have InferRouter select providers by requested `Model` name instead of adding a dedicated field
Rejected because InferRouter's `Model` field is documented as an optional override of the model sent to whichever provider is selected — not a provider-selection key. Repurposing it would overload an existing, differently-intended field for existing callers.

## References
- ADR-004 — `ILLMProvider` abstraction for all LLM calls (interface boundary, unchanged; "Option B" reasoning revisited above)
- InferRouter ADR-011 — Explicit Provider Targeting (companion decision this ADR depends on)
- `docs/specs/19-llm-provider-groq.md`, `docs/specs/20-llm-provider-llamasharp.md` — superseded implementation
- See `llm-strategy.md` for role-to-provider mapping conventions
