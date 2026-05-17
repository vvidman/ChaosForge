---
category: specs
title: "Docker and docker-compose"
branch: "docker"
status: ready
date: "2026-04-21"
related_domain: []
related_adr: []
---

# Feature Spec — Docker and docker-compose

<!-- Reference this file in the implementation agent with: implement @docs/specs/43-docker.md -->

---

## Context

Currently the application can only be run with `dotnet run` + `npm run dev` separately.
This spec adds a `Dockerfile` for the .NET API and a `docker-compose.yml` that starts
both the API and serves the built React frontend, making it possible to run the entire
stack with a single `docker compose up`. Depends on: all backend specs, spec 32.

---

## Domain Impact

- New or modified entity: none
- New domain event: none
- New interface: none

---

## Architecture Decisions

- **API Dockerfile:** multi-stage build (SDK image for build, ASP.NET runtime image
  for publish). Output: minimal runtime image.
- **Frontend:** built with `npm run build` into `web/dist/`, served as static files by
  the .NET API using `UseStaticFiles` + `MapFallbackToFile("index.html")`. The React
  app is NOT served from a separate container — it is embedded in the API image.
  This simplifies the setup: one container, one port.
- **Build context:** the `Dockerfile` and `docker-compose.yml` live in `ChaosForge/`,
  alongside `ChaosForge.slnx`. Docker build context is `ChaosForge/`. All `COPY`
  paths in the Dockerfile are relative to `ChaosForge/`.
- **SQLite:** the database file is stored in a Docker volume mounted at
  `/data/chaosforge.db`. Connection string uses this path.
- **LlamaSharp:** model file is NOT bundled in the image — it must be provided via a
  volume mount. If no model file is present, the `LlamaSharpLlmProvider` fails fast at
  startup (existing behaviour).
- **Base images:**
  - Build: `mcr.microsoft.com/dotnet/sdk:10.0`
  - Runtime: `mcr.microsoft.com/dotnet/aspnet:10.0`
  - Both use `-alpine` variant to minimize image size.
- **docker-compose.yml** defines one service: `chaosforge`.
  Environment variables override `appsettings.json` values.
- **Groq API key:** passed via environment variable `Groq__ApiKey` (double underscore
  = .NET configuration section separator). Never in the image.
- **LlamaSharp model path:** passed via `LlamaSharp__ModelPath` env var, default empty
  (feature disabled if not set).

### Dockerfile (multi-stage)

```dockerfile
# Stage 1: build frontend
FROM node:22-alpine AS fe-build
WORKDIR /app/web
COPY web/package*.json ./
RUN npm ci
COPY web/ .
RUN npm run build

# Stage 2: build backend
FROM mcr.microsoft.com/dotnet/sdk:10.0-alpine AS be-build
WORKDIR /app
COPY ChaosForge.slnx ./
COPY src/ src/
RUN dotnet publish src/ChaosForge.API/ChaosForge.API.csproj \
    -c Release -o /publish --no-self-contained

# Stage 3: runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-alpine AS runtime
WORKDIR /app
COPY --from=be-build /publish ./
COPY --from=fe-build /app/web/dist ./wwwroot
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ChaosForge.API.dll"]
```

### docker-compose.yml

```yaml
services:
  chaosforge:
    build:
      context: .
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
    volumes:
      - chaosforge-data:/data
      - ${LLAMA_MODEL_DIR:-/tmp}:/models:ro
    environment:
      - ConnectionStrings__DefaultConnection=Data Source=/data/chaosforge.db
      - Groq__ApiKey=${GROQ_API_KEY:-}
      - Groq__Model=${GROQ_MODEL:-llama-3.3-70b-versatile}
      - LlamaSharp__ModelPath=${LLAMA_MODEL_PATH:-}
      - LlamaSharp__MaxTokens=${LLAMA_MAX_TOKENS:-512}
      - Agents__PollingIntervalMs=${POLLING_INTERVAL_MS:-3000}

volumes:
  chaosforge-data:
```

### .env.docker.example

```
GROQ_API_KEY=your-key-here
GROQ_MODEL=llama-3.3-70b-versatile
LLAMA_MODEL_DIR=/path/to/your/models
LLAMA_MODEL_PATH=/models/your-model.gguf
POLLING_INTERVAL_MS=3000
```

---

## Implementation Scope — What must be done

- [ ] Create `Dockerfile` in `ChaosForge/` (alongside `ChaosForge.slnx`) using the multi-stage structure above
- [ ] Create `docker-compose.yml` in `ChaosForge/`
- [ ] Create `.env.docker.example` in `ChaosForge/`
- [ ] Update `ChaosForge/.gitignore` to exclude `.env.docker`
- [ ] Update `src/ChaosForge.API/Program.cs` to serve static files:
  ```csharp
  app.UseStaticFiles();
  // ... after all API routes:
  app.MapFallbackToFile("index.html");
  ```
- [ ] Update `web/vite.config.ts` to set `base: '/'` (already default, verify)
- [ ] Create `docs/toolchain/docker.md` explaining how to build and run
  (run `docker compose up --build` from inside `ChaosForge/`):
  - `docker compose up --build`
  - How to set GROQ_API_KEY
  - How to mount a LlamaSharp model file
- [ ] Verify: `docker compose build` succeeds
- [ ] Verify: `docker compose up` starts and API responds on `http://localhost:8080`

---

## Out of Scope — What must NOT be done

- Do not add nginx as a reverse proxy — the API serves static files directly
- Do not add health check endpoints — not required for demo
- Do not add HTTPS termination in Docker — run behind a reverse proxy in production

---

## Test Expectations

- Unit tests required for: none
- Edge cases to cover: `GROQ_API_KEY` not set — app starts but LLM calls fail with
  a clear error (existing behaviour, no code change needed)

---

## Open Questions

- None.
