# start-backend.ps1 — Build & run GiveAID v2 WebApi on port 5231.
# Logs to scripts/backend.log, writes PID to scripts/backend.pid.
# Auto-detects path: GiveAID.Client/.. -> project-NGO.v2/

$ErrorActionPreference = 'Stop'

# Resolve v2 project root (2 levels up from scripts/):
# scripts/ -> GiveAID.Client/ -> project NGO/ -> Desktop/
# Desktop/ has both "project NGO" (client) and "project NGO.v2" (solution)
$clientRoot = Resolve-Path (Join-Path $PSScriptRoot '..')                # GiveAID.Client
$clientParent = (Get-Item $clientRoot).Parent.FullName                   # project NGO
$desktop      = (Get-Item $clientParent).Parent.FullName                 # Desktop
$v2Root       = Join-Path $desktop 'project NGO.v2'

if (-not (Test-Path $v2Root)) {
    # Fallback: same as clientParent (legacy layout)
    $v2Root = $clientParent
}

$apiProject   = Join-Path $v2Root 'src\WebApi\GiveAID.V2.WebApi.csproj'
$apiDll       = Join-Path $v2Root 'src\WebApi\bin\Debug\net10.0\GiveAID.V2.WebApi.dll'
$slnx         = Join-Path $v2Root 'GiveAID.V2.slnx'
$logFile      = Join-Path $PSScriptRoot 'backend.log'
$pidFile      = Join-Path $PSScriptRoot 'backend.pid'
$apiPort      = 5000

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

# --- Start WebApi ---
Write-Host "[start-backend] Starting WebApi on http://localhost:$apiPort..." -ForegroundColor Cyan
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName               = $dotnet.Source
# Use Arguments string (cross-compatible: works in both PS 5.1 and PS 7+)
# Note: paths with spaces must be quoted
$psi.Arguments              = "run --project `"$apiProject`" --no-build --urls http://localhost:$apiPort"
$psi.UseShellExecute        = $false
$psi.WorkingDirectory       = $v2Root
# Start WebApi in a NEW VISIBLE WINDOW so user can see errors
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$cmd = "cd /d `"$v2Root`" && dotnet run --project `"$apiProject`" --no-build --urls http://localhost:$apiPort"
Start-Process cmd -ArgumentList "/c title Backend-5000 && $cmd && pause" -WorkingDirectory $v2Root -WindowStyle Normal

Write-Host "[start-backend] WebApi started on http://localhost:$apiPort" -ForegroundColor Green
