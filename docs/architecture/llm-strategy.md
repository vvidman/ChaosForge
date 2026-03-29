---
category: architecture
principle: "LLM Provider Strategy"
last_updated: "2026-03-29"
related_adr: [ADR-004, ADR-007]
---

# LLM Provider Strategy

## What it is
`ILLMProvider` (Domain interface) abstracts all LLM calls. Three implementations:
`LlamaSharpProvider` (local CPU), `GroqProvider` (free online, Llama 3.3 70B),
`OpenAICompatibleProvider` (generic fallback). Role-to-provider mapping is configurable
per project at runtime.

## Why we apply it
Complex reasoning roles need cloud models; repetitive roles run locally.
Provider swaps require no changes to Application or Domain layers.

## How we apply it

### Default role mapping

| Roles | Provider | Reason |
|---|---|---|
| BA, Architect, ScrumMaster | Groq | Complex reasoning |
| Developer, Tester, Reviewer, TechnicalWriter | LlamaSharp | Repetitive, CPU-feasible |

### Contract
`ILLMProvider` returns a completion string or throws a typed exception.
Never returns null. Never swallows errors silently.

### Configuration
Mapping is resolved inside `AddInfrastructureServices(configuration)` from project-level
settings. Handlers receive `ILLMProvider` — they never know which provider is active.

## Rules
- **Must**: Application handlers call `ILLMProvider` only — never a concrete provider type
- **Must**: provider-to-role mapping lives in DI configuration, not in any handler
- **Must**: all `ILLMProvider` implementations throw on failure — null returns are contract violations
- **?** Is any provider selection logic inside an Application handler? If yes → move to `AddInfrastructureServices`.
- **?** Is any LLM SDK namespace imported outside Infrastructure? If yes → DIP violation.

## Anti-patterns

### Direct SDK reference in Application
`using LLamaSharp;` or `using Groq;` anywhere in Application — violates DIP.
All LLM calls go through `ILLMProvider`.

### Hardcoded provider assignment in a handler
`if (role == AgentRole.Developer) new LlamaSharpProvider()` — violates OCP and DIP.
Provider assignment is exclusively a DI configuration concern.

## References
- See `solid.md` DIP and LSP sections — interface contract rules
- See `agent-design.md` — role definitions and singleton/multi-instance breakdown
