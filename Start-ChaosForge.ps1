#Requires -Version 7.0
<#
.SYNOPSIS
    Bootstrap and run ChaosForge via Docker.

.DESCRIPTION
    Checks prerequisites, creates .env.docker from .env.docker.example if missing,
    makes sure INFERROUTER_BASE_URL is set, then runs docker compose up --build.

.PARAMETER Detach
    Run containers in background (-d).

.PARAMETER Rebuild
    Force image rebuild even if nothing changed (--no-cache).

.PARAMETER Down
    Stop and remove containers instead of starting.

.PARAMETER Wipe
    Stop containers AND remove the data volume (destroys database).

.EXAMPLE
    .\Start-ChaosForge.ps1
    .\Start-ChaosForge.ps1 -Detach
    .\Start-ChaosForge.ps1 -Down
    .\Start-ChaosForge.ps1 -Wipe
#>
param(
    [switch]$Detach,
    [switch]$Rebuild,
    [switch]$Down,
    [switch]$Wipe
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptDir   = $PSScriptRoot
$EnvFile     = Join-Path $ScriptDir '.env.docker'
$EnvExample  = Join-Path $ScriptDir '.env.docker.example'
$ComposeFile = Join-Path $ScriptDir 'docker-compose.yml'

# ── Helpers ────────────────────────────────────────────────────────────────────

function Write-Step([string]$msg) { Write-Host "==> $msg" -ForegroundColor Cyan }
function Write-Ok([string]$msg)   { Write-Host " ok  $msg" -ForegroundColor Green }
function Write-Warn([string]$msg) { Write-Host "warn $msg" -ForegroundColor Yellow }
function Abort([string]$msg)      { Write-Host "ERR  $msg" -ForegroundColor Red; exit 1 }

# ── Prerequisite checks ────────────────────────────────────────────────────────

Write-Step "Checking prerequisites"

if (-not (Get-Command docker -ErrorAction SilentlyContinue)) {
    Abort "docker not found. Install Docker Desktop: https://docs.docker.com/desktop/"
}

$dockerInfo = docker info 2>&1
if ($LASTEXITCODE -ne 0) {
    Abort "Docker daemon not running. Start Docker Desktop and try again."
}
Write-Ok "Docker is running"

# ── Down / Wipe ────────────────────────────────────────────────────────────────

if ($Down -or $Wipe) {
    Write-Step "Stopping containers"
    $downArgs = @('compose', '--env-file', $EnvFile, 'down')
    if ($Wipe) {
        $downArgs += '-v'
        Write-Warn "Volume chaosforge-data will be removed (database wiped)"
    }
    & docker @downArgs
    Write-Ok "Done"
    exit 0
}

# ── .env.docker setup ──────────────────────────────────────────────────────────

Write-Step "Checking .env.docker"

if (-not (Test-Path $EnvFile)) {
    if (-not (Test-Path $EnvExample)) {
        Abort ".env.docker.example not found. Repo may be incomplete."
    }
    Copy-Item $EnvExample $EnvFile
    Write-Warn ".env.docker created from example. Configure it before production use."
}
Write-Ok ".env.docker present"

# Read current env file into a hashtable
$envVars = @{}
Get-Content $EnvFile | Where-Object { $_ -match '^\s*[^#].*=' } | ForEach-Object {
    $parts = $_ -split '=', 2
    $envVars[$parts[0].Trim()] = $parts[1].Trim()
}

# ── InferRouter URL ────────────────────────────────────────────────────────────

$inferUrl = $envVars['INFERROUTER_BASE_URL']
if ([string]::IsNullOrWhiteSpace($inferUrl)) {
    Write-Warn "INFERROUTER_BASE_URL is not set in .env.docker"
    $inferUrl = Read-Host "  Enter InferRouter base URL as seen from the container (e.g. http://host.docker.internal:5100)"
    if ([string]::IsNullOrWhiteSpace($inferUrl)) {
        Abort "INFERROUTER_BASE_URL is required - the API refuses to start without it."
    }
    $content = Get-Content $EnvFile -Raw
    if ($content -match '(?m)^INFERROUTER_BASE_URL=') {
        $content = [regex]::Replace($content, '(?m)^INFERROUTER_BASE_URL=.*$', { "INFERROUTER_BASE_URL=$inferUrl" })
    } else {
        $content = $content.TrimEnd() + [Environment]::NewLine + "INFERROUTER_BASE_URL=$inferUrl" + [Environment]::NewLine
    }
    Set-Content $EnvFile $content -NoNewline
    Write-Ok "INFERROUTER_BASE_URL saved to .env.docker"
} else {
    Write-Ok "InferRouter: $inferUrl"
}

if ($inferUrl -match '://(localhost|127\.0\.0\.1)([:/]|$)') {
    Write-Warn "INFERROUTER_BASE_URL points to localhost - inside the container that is the container itself."
    Write-Warn "Use http://host.docker.internal:<port> for an InferRouter running on this machine."
}

$legacyKeys = @('GROQ_API_KEY', 'GROQ_MODEL', 'LLAMA_MODEL_DIR', 'LLAMA_MODEL_PATH') |
    Where-Object { $envVars.ContainsKey($_) }
if ($legacyKeys) {
    Write-Warn "Ignoring legacy keys in .env.docker (LLM routing moved to InferRouter, see ADR-011): $($legacyKeys -join ', ')"
}

# ── Build + run ────────────────────────────────────────────────────────────────

Write-Step "Building and starting ChaosForge"

$composeArgs = @('compose', '--env-file', $EnvFile, 'up', '--build')
if ($Detach)  { $composeArgs += '-d' }
if ($Rebuild) { $composeArgs += '--no-cache' }

Write-Host ""
Write-Host "  docker $($composeArgs -join ' ')" -ForegroundColor DarkGray
Write-Host ""

& docker @composeArgs

if ($LASTEXITCODE -ne 0) {
    Abort "docker compose failed (exit $LASTEXITCODE)"
}

if ($Detach) {
    Write-Host ""
    Write-Ok "ChaosForge running in background"
    Write-Host "  App:  http://localhost:8080" -ForegroundColor White
    Write-Host "  API:  http://localhost:8080/api/projects" -ForegroundColor White
    Write-Host "  Stop: .\Start-ChaosForge.ps1 -Down" -ForegroundColor DarkGray
}
