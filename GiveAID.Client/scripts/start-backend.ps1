# start-backend.ps1 — Build & run GiveAID v2 WebApi on port 5231.
# Logs to scripts/backend.log, writes PID to scripts/backend.pid.
# Auto-detects path: GiveAID.Client/.. -> project-NGO.v2/

$ErrorActionPreference = 'Stop'

# Resolve v2 project root (2 levels up from scripts/, then look for NGO.v2 sibling):
# scripts/ -> GiveAID.Client/ -> project NGO/ -> Desktop/
# Desktop/ has both "project NGO" (old) and "project NGO.v2" (v2 solution)
$clientRoot   = Resolve-Path (Join-Path $PSScriptRoot '..')                # GiveAID.Client
$clientParent = (Get-Item $clientRoot).Parent.FullName                   # project NGO
$desktop      = (Get-Item $clientParent).Parent.FullName                 # Desktop

# Dynamically locate the v2 sibling (the repo root for NGO.v2)
# Try common v2 naming patterns first, then fall back to searching Desktop
$v2Patterns = @(
    'project NGO.v2',
    'project NGO v2',
    'NGO.v2',
    'care4kids',
    'project-NGO.v2'
)
$v2Root = $null
foreach ($pattern in $v2Patterns) {
    $candidate = Join-Path $desktop $pattern
    if (Test-Path $candidate) {
        # Verify this is actually the v2 solution by checking for the .slnx or csproj
        if ((Test-Path (Join-Path $candidate 'GiveAID.V2.slnx')) -or
            (Test-Path (Join-Path $candidate 'src\WebApi\GiveAID.V2.WebApi.csproj'))) {
            $v2Root = $candidate
            break
        }
    }
}

# Final validation — verify the expected WebApi project file exists
if (-not $v2Root) {
    Write-Host "[start-backend] ERROR: Could not locate NGO.v2 project root." -ForegroundColor Red
    Write-Host "[start-backend] Searched Desktop: $desktop" -ForegroundColor Yellow
    Write-Host "[start-backend] Tried patterns: $($v2Patterns -join ', ')" -ForegroundColor Yellow
    Write-Host "[start-backend] None of those directories contain GiveAID.V2.WebApi.csproj" -ForegroundColor Yellow
    Write-Host "[start-backend] HINT: Ensure the repo is cloned at one of the above names under Desktop," -ForegroundColor Yellow
    Write-Host "[start-backend]       OR move this script's 'GiveAID.Client' folder to be a sibling of the v2 root." -ForegroundColor Yellow
    exit 1
}

$apiProject = Join-Path $v2Root 'src\WebApi\GiveAID.V2.WebApi.csproj'
$apiDll       = Join-Path $v2Root 'src\WebApi\bin\Debug\net10.0\GiveAID.V2.WebApi.dll'
$slnx         = Join-Path $v2Root 'GiveAID.V2.slnx'
$logFile      = Join-Path $PSScriptRoot 'backend.log'
$pidFile      = Join-Path $PSScriptRoot 'backend.pid'
$apiPort      = 5231

Write-Host "[start-backend] v2 root: $v2Root" -ForegroundColor Cyan

# --- Pre-flight: free API port ---
Write-Host "[start-backend] Checking port $apiPort..." -ForegroundColor Cyan
Get-NetTCPConnection -LocalPort $apiPort -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    $proc = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "[start-backend] Port $apiPort held by $($proc.ProcessName) (PID $($proc.Id)) - killing" -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
}
Start-Sleep -Seconds 1

# Kill previous backend from .pid
if (Test-Path $pidFile) {
    $oldPid = Get-Content $pidFile -ErrorAction SilentlyContinue
    if ($oldPid) {
        try {
            Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
            Write-Host "[start-backend] Killed previous backend (PID $oldPid)" -ForegroundColor Yellow
        } catch {}
    }
    Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
}

# --- Check dotnet ---
$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "[start-backend] ERROR: dotnet CLI not found in PATH" -ForegroundColor Red
    Write-Host "[start-backend] Install .NET 8 SDK from https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    exit 1
}

# --- Build if DLL missing or stale ---
$needsBuild = $false
if (-not (Test-Path $apiDll)) {
    $needsBuild = $true
} else {
    $csFiles = Get-ChildItem $v2Root -Recurse -Filter '*.cs' -ErrorAction SilentlyContinue |
               Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' }
    $dllTime = (Get-Item $apiDll).LastWriteTime
    foreach ($f in $csFiles) {
        if ($f.LastWriteTime -gt $dllTime) { $needsBuild = $true; break }
    }
}

if ($needsBuild) {
    Write-Host "[start-backend] Building GiveAID.V2.WebApi..." -ForegroundColor Cyan
    & dotnet build $apiProject -c Debug --nologo -v minimal 2>&1 | Tee-Object -FilePath $logFile -Append
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[start-backend] BUILD FAILED. Check $logFile" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[start-backend] DLL up-to-date, skipping build" -ForegroundColor Green
}

# Start WebApi ---
Write-Host "[start-backend] Starting WebApi on http://localhost:$apiPort..." -ForegroundColor Cyan

# Run dotnet directly with -NoNewWindow so the console output goes to this PowerShell
# and npm sees this process as running
$proc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project `"$apiProject`" --no-build --urls http://localhost:$apiPort" `
    -WorkingDirectory $v2Root `
    -NoNewWindow `
    -PassThru

if ($proc) {
    Set-Content -Path $pidFile -Value $proc.Id
    Write-Host "[start-backend] WebApi started, PID $($proc.Id)" -ForegroundColor Green
    # Wait for the dotnet process to exit - this keeps npm's concurrently happy
    $proc.WaitForExit()
    Write-Host "[start-backend] WebApi stopped (PID $($proc.Id))" -ForegroundColor Yellow
} else {
    Write-Host "[start-backend] WARNING: Could not get process handle" -ForegroundColor Yellow
}
