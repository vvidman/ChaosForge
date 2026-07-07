---
category: architecture
principle: "LLM Provider Strategy"
last_updated: "2026-07-07"
related_adr: [ADR-004, ADR-011]
---

# LLM Provider Strategy

## What it is
`ILLMProvider` (Application interface) abstracts all LLM calls. A single implementation,
`InferRouterLlmProvider`, calls the companion InferRouter service's OpenAI-compatible
`POST /v1/chat/completions` endpoint. Two keyed instances of the same class carry different
`preferredProviderName` values (`"groq"`, `"local-llama"`), preserving role-based routing
preference while InferRouter owns actual provider fallback, rate-limiting, and health checks.

## Why we apply it
Complex reasoning roles prefer cloud models; repetitive roles prefer local inference.
InferRouter's `PreferredProviderName` field lets ChaosForge keep that proactive preference
without maintaining its own provider SDK integrations. Provider swaps require no changes to
Application or Domain layers.

## How we apply it

### Default role mapping

| Roles | Keyed provider | Preferred provider name | Reason |
|---|---|---|---|
| BA, Architect, ScrumMaster | `cloud-preferred` | `groq` | Complex reasoning |
| Developer, Tester, Reviewer, TechnicalWriter | `local-preferred` | `local-llama` | Repetitive, CPU-feasible |

### Contract
`ILLMProvider` returns a completion string or throws a typed exception.
Never returns null. Never swallows errors silently.

### Configuration
`InferRouterOptions.BaseUrl` (config section `"InferRouter"`) points at the InferRouter
instance. Mapping is resolved inside `AddInfrastructure(configuration)` from project-level
settings. Handlers receive `ILLMProvider` — they never know which provider is active.

## Rules
- **Must**: Application handlers call `ILLMProvider` only — never a concrete provider type
- **Must**: provider-to-role mapping lives in DI configuration, not in any handler
- **Must**: all `ILLMProvider` implementations throw on failure — null returns are contract violations
- **?** Is any provider selection logic inside an Application handler? If yes → move to `AddInfrastructure`.
- **?** Is any LLM SDK namespace imported outside Infrastructure? If yes → DIP violation.

## Anti-patterns

### Direct SDK reference in Application
`using InferRouter;` or any provider SDK namespace anywhere in Application — violates DIP.
All LLM calls go through `ILLMProvider`.

### Hardcoded provider assignment in a handler
`if (role == AgentRole.Developer) new InferRouterLlmProvider(...)` — violates OCP and DIP.
Provider assignment is exclusively a DI configuration concern.

## References
- See `solid.md` DIP and LSP sections — interface contract rules
- See `agent-design.md` — role definitions and singleton/multi-instance breakdown
- [ADR-011](../adr/011-inferrouter-integration.md) — InferRouter integration decision
