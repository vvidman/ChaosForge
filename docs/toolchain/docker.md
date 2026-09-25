---
category: toolchain
title: Docker
covers: ["Docker", "docker-compose", "container", "static files", "InferRouter", "host.docker.internal", "production build"]
---

# Docker

Runs the full stack (API + React SPA) as a single container on port 8080.

## Prerequisites

- Docker Desktop (or Docker Engine + Compose plugin)
- A running InferRouter instance reachable from the container (required — the API
  refuses to start without `InferRouter:BaseUrl`, see `configuration.md`)

## Setup

On Windows, `.\Start-ChaosForge.ps1` does the steps below and prompts for anything missing.

```bash
# From the repository root (same directory as docker-compose.yml)
cp .env.docker.example .env.docker
```

Edit `.env.docker`:

```
INFERROUTER_BASE_URL=http://host.docker.internal:5100
```

The URL is resolved **inside the container**: `localhost` would be the container itself.
Use `host.docker.internal` for an InferRouter running on the Docker host (the compose file maps
it via `host-gateway`, so this also works on Linux engines), or a LAN address otherwise.

## Build and run

```bash
docker compose --env-file .env.docker up --build
```

- API: `http://localhost:8080/api/projects`
- React SPA: `http://localhost:8080`

## Stopping

```bash
docker compose --env-file .env.docker down
```

Data persists in the `chaosforge-data` named volume. To wipe it:

```bash
docker compose --env-file .env.docker down -v
```

## Architecture notes

- React build output (`web/dist/`) is copied into `wwwroot/` in the API image.
- `UseStaticFiles` + `MapFallbackToFile("index.html")` in `Program.cs` serve the SPA.
- SQLite database lives at `/data/chaosforge.db` inside the container (volume-backed).
- No HTTPS termination in Docker — place a reverse proxy in front for production.
