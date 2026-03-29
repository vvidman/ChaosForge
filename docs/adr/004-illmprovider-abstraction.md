---
category: adr
id: "004"
title: "ILLMProvider abstraction for all LLM calls"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [llm-strategy, solid]
---

# ADR-004: ILLMProvider abstraction for all LLM calls

## Status
`accepted`

## Context
ChaosForge supports multiple LLM backends: LlamaSharp (local CPU), Groq (free cloud),
and a generic OpenAI-compatible fallback. Different roles benefit from different providers.
Application handlers must not be coupled to any specific SDK, and the active provider for
a role must be configurable without code changes.

## Decision
`ILLMProvider` is declared in the Domain layer. All three implementations live in Infrastructure.
Role-to-provider mapping is resolved exclusively in `AddInfrastructureServices(configuration)`.
Application handlers receive `ILLMProvider` via constructor injection — they never reference
a concrete provider type or any LLM SDK namespace.

## Consequences

**Positive:**
- Providers are swappable per role without touching Application or Domain
- Application handlers are unit-testable with a mock `ILLMProvider`
- New providers (e.g. Anthropic, Mistral) require only a new Infrastructure class and a
  DI registration change

**Trade-offs:**
- The abstraction must be kept simple: `ILLMProvider` exposes a completion method, not
  provider-specific features (streaming, function calling). Advanced features require
  interface evolution or a separate specialized interface.

## Alternatives Considered

### Option A: Direct SDK calls from Application handlers
Inject `LlamaSharpProvider` or `GroqClient` directly into handlers.
Rejected because it violates DIP, makes handlers untestable without real LLM infrastructure,
and couples the Application layer to specific SDK versions.

### Option B: Generic HTTP client wrapper (OpenAI-compatible only)
One HTTP-based provider that works with any OpenAI-compatible endpoint, including local ones.
Rejected because LlamaSharp runs in-process (no HTTP overhead, lower latency for CPU inference)
and this advantage would be lost. The abstraction preserves the option to use in-process
inference where it matters.

## References
- See `llm-strategy.md` for role-to-provider mapping and contract rules
- See `solid.md` DIP and LSP sections
