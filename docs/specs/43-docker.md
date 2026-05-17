---
category: specs
title: "Docker and docker-compose"
branch: "docker"
status: ready
date: "2026-04-25"
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
stack with a single `docker compose up`. Depends on: all backend and frontend specs.

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
  volume mount. If no model file is present, the app starts normally (graceful
  degradation is implemented in spec 44).
- **Base images:**
  - Build: `mcr.microsoft.com/dotnet/sdk:10.0-alpine`
  - Runtime: `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`
- **docker-compose.yml** defines one service: `chaosforge`.
  Environment variables override `appsettings.json` values using .NET's `__` separator.
- **Groq API key:** passed via environment variable `Groq__ApiKey`. Never in the image.
- **`tailwindcss-animate`:** the frontend uses `animate-in` Tailwind utility classes
  (in `ToastContainer.tsx`) which require the `tailwindcss-animate` plugin. Add it
  before building the frontend: `npm install tailwindcss-animate` + register in
  `tailwind.config.ts`.

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
    -c Release -o /publish

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

### Frontend: tailwindcss-animate

- [ ] In `ChaosForge/web/`: `npm install tailwindcss-animate`
- [ ] Register in `web/tailwind.config.ts`:
  ```typescript
  plugins: [
    require('@tailwindcss/typography'),
    require('tailwindcss-animate'),
    // existing prefers-reduced-motion plugin...
  ]
  ```

### Program.cs: static files wiring

- [ ] Update `src/ChaosForge.API/Program.cs` to serve the React SPA.
  The order of middleware and route registration is critical:

  ```csharp
  app.UseStaticFiles();          // serve wwwroot files (React build output)
  app.UseHttpsRedirection();
  app.UseCors(...);

  // ... all API route registrations (MapProjectEndpoints, etc.) ...
  // ... MapHub<ChaosForgeHub>(...) ...

  // SPA fallback MUST be last — after all API routes
  app.MapFallbackToFile("index.html");

  app.Run();
  ```

  If `MapFallbackToFile` is placed before API routes, all API requests will return
  `index.html` instead of JSON. It must be the last route registered.

### Docker files

- [ ] Create `Dockerfile` in `ChaosForge/` using the multi-stage structure above
- [ ] Create `docker-compose.yml` in `ChaosForge/`
- [ ] Create `.env.docker.example` in `ChaosForge/`
- [ ] Update `ChaosForge/.gitignore` to exclude `.env.docker`

### Documentation

- [ ] Create `docs/toolchain/docker.md` explaining:
  - Prerequisites (Docker, optional: LlamaSharp model file)
  - How to set GROQ_API_KEY (copy `.env.docker.example` → `.env.docker`, fill in key)
  - How to mount a LlamaSharp model file
  - `docker compose up --build` run from inside `ChaosForge/`

### Verification

- [ ] `npm run build` in `web/` succeeds — zero errors, `animate-in` classes present
- [ ] `docker compose build` succeeds
- [ ] `docker compose up` starts, API responds on `http://localhost:8080/api/projects`
- [ ] Navigate to `http://localhost:8080` in browser — React app loads
- [ ] `dotnet build` — zero warnings, zero errors

---

## Out of Scope — What must NOT be done

- Do not add nginx as a reverse proxy — the API serves static files directly
- Do not add health check endpoints
- Do not add HTTPS termination in Docker — use a reverse proxy in production
- Do not add `--no-self-contained` to `dotnet publish` — framework-dependent publish
  is the default and correct for the aspnet runtime image

---

## Test Expectations

- Unit tests required for: none
- Edge cases to cover: `GROQ_API_KEY` not set — app starts, LLM calls fail at runtime
  with a descriptive error (implemented in spec 44)

---

## Open Questions

- None.
