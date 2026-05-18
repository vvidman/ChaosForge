---
category: toolchain
title: Docker
covers: ["Docker", "docker-compose", "container", "static files", "Groq API key", "LlamaSharp model", "production build"]
---

# Docker

Runs the full stack (API + React SPA) as a single container on port 8080.

## Prerequisites

- Docker Desktop (or Docker Engine + Compose plugin)
- A Groq API key (optional — app starts without one, LLM calls fail at runtime)
- A GGUF model file on the host (optional — LlamaSharp is disabled if path is absent)

## Setup

```bash
# From ChaosForge/ (same directory as docker-compose.yml)
cp .env.docker.example .env.docker
```

Edit `.env.docker` and set at minimum:

```
GROQ_API_KEY=gsk_...your_key_here...
```

If you want local LlamaSharp inference, also set:

```
LLAMA_MODEL_DIR=/absolute/path/to/folder/containing/model
LLAMA_MODEL_PATH=/models/your-model.gguf
```

`LLAMA_MODEL_DIR` is the host directory. It is mounted read-only at `/models` inside the
container. `LLAMA_MODEL_PATH` must use the `/models/...` path (container side).

## Build and run

```bash
docker compose --env-file .env.docker up --build
```

- API: `http://localhost:8080/api/projects`
- React SPA: `http://localhost:8080`

## Stopping

```bash
docker compose down
```

Data persists in the `chaosforge-data` named volume. To wipe it:

```bash
docker compose down -v
```

## Architecture notes

- React build output (`web/dist/`) is copied into `wwwroot/` in the API image.
- `UseStaticFiles` + `MapFallbackToFile("index.html")` in `Program.cs` serve the SPA.
- SQLite database lives at `/data/chaosforge.db` inside the container (volume-backed).
- No HTTPS termination in Docker — place a reverse proxy in front for production.
