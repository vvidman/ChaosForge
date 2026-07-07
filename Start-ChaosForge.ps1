#Requires -Version 7.0
<#
.SYNOPSIS
    Bootstrap and run ChaosForge via Docker.

.DESCRIPTION
    Checks prerequisites, creates .env.docker from .env.docker.example if missing,
    prompts for GROQ_API_KEY, then runs docker compose up --build.

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

# ── Groq API key prompt ────────────────────────────────────────────────────────

$groqKey = $envVars['GROQ_API_KEY']
if ([string]::IsNullOrWhiteSpace($groqKey) -or $groqKey -eq 'your-key-here') {
    Write-Warn "GROQ_API_KEY is not set in .env.docker"
    $entered = Read-Host "  Enter Groq API key (leave blank to skip — LLM calls will fail at runtime)"
    if (-not [string]::IsNullOrWhiteSpace($entered)) {
        # Update the key in the file
        $content = Get-Content $EnvFile -Raw
        $content = $content -replace '(?m)^GROQ_API_KEY=.*$', "GROQ_API_KEY=$entered"
        Set-Content $EnvFile $content -NoNewline
        Write-Ok "GROQ_API_KEY saved to .env.docker"
    } else {
        Write-Warn "Skipped — app will start but LLM agent calls will fail"
    }
} else {
    Write-Ok "GROQ_API_KEY is set"
}

# ── LlamaSharp model (optional) ────────────────────────────────────────────────

$modelDir  = $envVars['LLAMA_MODEL_DIR']
$modelPath = $envVars['LLAMA_MODEL_PATH']

if ([string]::IsNullOrWhiteSpace($modelDir) -or $modelDir -eq '/path/to/your/models') {
    Write-Warn "LLAMA_MODEL_DIR not configured — local inference disabled (Groq only)"
} else {
    if (Test-Path $modelDir) {
        Write-Ok "LlamaSharp model dir: $modelDir"
    } else {
        Write-Warn "LLAMA_MODEL_DIR '$modelDir' does not exist — local inference disabled"
    }
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
