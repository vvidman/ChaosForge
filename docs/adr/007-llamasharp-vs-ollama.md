---
category: adr
id: "007"
title: "LlamaSharp for local inference over Ollama HTTP"
status: accepted
date: "2026-03-29"
supersedes: null
superseded_by: null
related_principles: [llm-strategy]
---

# ADR-007: LlamaSharp for local inference over Ollama HTTP

## Status
`accepted`

## Context
ChaosForge runs LLM inference locally on CPU (no GPU). The local provider must run
in-process with the .NET host — no external processes or Docker containers. The
`ILLMProvider` abstraction means the choice of local backend is Infrastructure-only.

## Decision
Use LlamaSharp for local inference. LlamaSharp runs in-process via llama.cpp bindings —
no separate process, no HTTP overhead, no Ollama daemon required. The CPU-only constraint
is explicitly supported.

## Consequences

**Positive:**
- Zero external process dependency: `dotnet run` is sufficient
- Lower latency than HTTP-based local providers (no serialization round-trip)
- llama.cpp CPU inference is well-maintained and supports GGUF models

**Trade-offs:**
- Model management (downloading, loading) is handled in code, not via Ollama's CLI
- Upgrading llama.cpp version requires a NuGet update, not just a daemon restart
- If GPU support is needed later, LlamaSharp supports it — but this is not a current requirement

## Alternatives Considered

### Option A: Ollama via HTTP (OpenAI-compatible endpoint)
Ollama manages models and exposes a REST API. Provider-agnostic: the same HTTP client
works for Ollama, LM Studio, or any OpenAI-compatible local server.
Rejected because it requires Ollama to be running as a separate process — violating the
"single `dotnet run`" setup constraint. Also adds HTTP overhead for every inference call.

### Option B: OpenAICompatibleProvider pointed at a local server
Use the generic HTTP provider for both local and cloud.
Rejected for the same reason as Option A: requires an external process. Kept as a
registered provider for future use cases where a separate inference server is acceptable.

## References
- See `llm-strategy.md` for role-to-provider mapping
- See ADR-004 for the `ILLMProvider` abstraction that makes this decision Infrastructure-only
