---
category: toolchain
tool: "LlamaSharp — Local LLM Inference"
last_updated: "2026-03-29"
---

# LlamaSharp — Local LLM Inference

## Purpose
LlamaSharp runs llama.cpp in-process with the .NET host — no separate process, no HTTP overhead.
Used for repetitive agent roles (Developer, Tester, Reviewer, TechnicalWriter) where local
CPU inference is sufficient. See ADR-007 for the rationale over Ollama.

## Prerequisites
- .NET 10 SDK
- ~4–8 GB free disk space for the model file
- CPU-only — no GPU required

## Setup

### 1. Download a GGUF model

Recommended: **Llama 3.2 3B Instruct Q4_K_M** (balance of speed and quality on CPU)

```bash
# Using huggingface-cli (pip install huggingface_hub)
huggingface-cli download \
  bartowski/Llama-3.2-3B-Instruct-GGUF \
  Llama-3.2-3B-Instruct-Q4_K_M.gguf \
  --local-dir ./models
```

Or download manually from: https://huggingface.co/bartowski/Llama-3.2-3B-Instruct-GGUF

### 2. Configure the model path

In `appsettings.Development.json` (never commit this file with real paths):

```json
{
  "LlamaSharp": {
    "ModelPath": "./models/Llama-3.2-3B-Instruct-Q4_K_M.gguf",
    "ContextSize": 4096,
    "GpuLayerCount": 0
  }
}
```

Or via user-secrets:

```bash
dotnet user-secrets set "LlamaSharp:ModelPath" "/absolute/path/to/model.gguf" \
  --project src/ChaosForge.API
```

### 3. Verify

```bash
dotnet run --project src/ChaosForge.API
```

On startup, LlamaSharp logs the model name and context size. If the model path is wrong,
it throws at DI registration time — not at first inference call.

## Common Tasks

### Switching to a different model
Update `LlamaSharp:ModelPath` in user-secrets or `appsettings.Development.json`.
No code changes required — the path is injected into `LlamaSharpProvider` via configuration.

### Adjusting context size
Larger context = more memory. Default 4096 tokens is sufficient for most agent prompts.
If a prompt exceeds context size, LlamaSharp truncates silently — increase `ContextSize` if
agent output quality degrades on long tasks.

```json
"LlamaSharp": {
  "ContextSize": 8192
}
```

## Troubleshooting

### `DllNotFoundException: libllama`
**Cause:** The LlamaSharp NuGet package includes native binaries, but the runtime target may not match.
**Fix:** Ensure `LLamaSharp.Backend.Cpu` NuGet package is referenced in `ChaosForge.Infrastructure`. This provides the CPU-only native lib.

### Model loads but output is garbage
**Cause:** Wrong prompt format for the model family. Llama 3 uses a specific chat template.
**Fix:** Verify `LlamaSharpProvider` uses the correct instruct template. Llama 3.2 Instruct expects `<|begin_of_text|><|start_header_id|>user<|end_header_id|>`.

### Out of memory during inference
**Cause:** Context size too large for available RAM, or model quantization too high.
**Fix:** Reduce `ContextSize` to 2048, or switch to a smaller quantization (Q3_K_S).

## References
- See ADR-007 for LlamaSharp vs. Ollama decision
- See `groq.md` for the cloud provider used by complex agent roles
- See `llm-strategy.md` (architecture) for role-to-provider mapping
