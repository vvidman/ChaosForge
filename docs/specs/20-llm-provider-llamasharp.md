---
category: specs
title: "LlamaSharp Local LLM Provider"
branch: "llm-llama"
status: done
date: "2026-04-07"
related_domain: []
related_adr: [004-illmprovider-abstraction, 007-llamasharp-vs-ollama]
---

# Feature Spec — LlamaSharp Local LLM Provider

<!-- Reference this file in the implementation agent with: implement @docs/specs/20-llm-provider-llamasharp.md -->

---

## Context

The Groq provider (spec 19) handles complex reasoning roles (BusinessAnalyst, Architect).
For repetitive, high-frequency roles (Developer, Tester, Reviewer, TechnicalWriter),
local CPU inference via LlamaSharp is used to avoid Groq free-tier rate limits. This spec
implements the second `ILlmProvider` and a role-based provider selector so agent handlers
can receive the correct provider without knowing which one they're using.
ADR-004 and ADR-007 define the reasoning.
Depends on: spec 19 (`ILlmProvider` interface).

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: `ILlmProviderSelector` (Application layer)

---

## Architecture Decisions

- `LlamaSharpLlmProvider` is `internal sealed`, lives in `Infrastructure/LLM/`.
- LlamaSharp NuGet package: `LLamaSharp` (already in `ChaosForge.Infrastructure.csproj`).
- Model path is configured via `appsettings.json` key `"LlamaSharp:ModelPath"`. The model
  file is not included in the repository.
- `LlamaSharpLlmProvider` wraps `LLamaWeights`, `LLamaContext`, and `InteractiveExecutor`
  from LlamaSharp. It is registered as a **singleton** because model loading is expensive.
- `CompleteAsync` builds a prompt from `systemPrompt + "\n" + userPrompt`, runs inference,
  and returns the generated text. Max tokens: configurable via `"LlamaSharp:MaxTokens"`
  (default 512).
- `ILlmProviderSelector` lives in `Application/Abstractions/`:
  ```csharp
  public interface ILlmProviderSelector
  {
      ILlmProvider GetProviderForRole(AgentRole role);
  }
  ```
- `LlmProviderSelector` (`internal sealed`) in `Infrastructure/LLM/` maps roles:
  - Groq: `BusinessAnalyst`, `Architect`, `ScrumMaster`
  - LlamaSharp: `Developer`, `Tester`, `Reviewer`, `TechnicalWriter`
- DI: both providers registered; `ILlmProviderSelector` registered as Scoped.
  LlamaSharpLlmProvider registered as Singleton and also keyed/named so the selector can
  resolve both from the container.
- If `LlamaSharp:ModelPath` is empty or the file does not exist, `LlamaSharpLlmProvider`
  throws `InvalidOperationException` at construction time — fail fast, not silently.

---

## Implementation Scope — What must be done

- [x] Create `Application/Abstractions/ILlmProviderSelector.cs` with interface above
- [x] Create `Infrastructure/LLM/LlamaSharpOptions.cs`:
  ```csharp
  public sealed class LlamaSharpOptions
  {
      public string ModelPath { get; init; } = string.Empty;
      public int MaxTokens { get; init; } = 512;
  }
  ```
- [x] Create `Infrastructure/LLM/LlamaSharpLlmProvider.cs` (`internal sealed`, `Singleton`):
  - Constructor: load model from `ModelPath`, validate file exists
  - `CompleteAsync`: run inference, return generated text as string
  - Dispose `LLamaWeights` / context on disposal (`IDisposable`)
- [x] Create `Infrastructure/LLM/LlmProviderSelector.cs` (`internal sealed`):
  - Injects both `GroqLlmProvider` and `LlamaSharpLlmProvider` (keyed or named)
  - `GetProviderForRole` returns the correct instance based on the role mapping above
- [x] Update `Infrastructure/DependencyInjection.cs`:
  - `services.Configure<LlamaSharpOptions>(configuration.GetSection("LlamaSharp"))`
  - Register `LlamaSharpLlmProvider` as Singleton
  - Register `ILlmProviderSelector` → `LlmProviderSelector` as Scoped
- [x] Add `LlamaSharp` config section to `appsettings.json`:
  ```json
  "LlamaSharp": {
    "ModelPath": "",
    "MaxTokens": 512
  }
  ```
- [x] Run `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not implement any agent logic — that is a future spec
- Do not add GPU support — CPU-only inference for this project
- Do not add streaming
- Do not download or bundle a model file

---

## Test Expectations

- Unit tests required for:
  - `LlmProviderSelector.GetProviderForRole`: verify each role maps to the correct
    provider type (mock both providers, assert reference equality)
- Edge cases to cover: `LlamaSharpLlmProvider` construction with empty `ModelPath` throws
  `InvalidOperationException` (test without actually loading a model)

---

## Open Questions

- None.
