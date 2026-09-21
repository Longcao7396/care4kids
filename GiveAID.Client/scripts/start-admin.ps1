# start-admin.ps1 — Build & run GiveAID v2 Admin console on port 5069.
# Logs to scripts/admin.log, writes PID to scripts/admin.pid.
# The Admin console talks to WebApi via HTTP (Api:BaseUrl in appsettings.json).

$ErrorActionPreference = 'Stop'

$clientRoot   = Resolve-Path (Join-Path $PSScriptRoot '..')                # GiveAID.Client
$clientParent = (Get-Item $clientRoot).Parent.FullName                   # project NGO
$desktop      = (Get-Item $clientParent).Parent.FullName                 # Desktop
$v2Root       = Join-Path $desktop 'project NGO.v2'

if (-not (Test-Path $v2Root)) {
    $v2Root = $clientParent
}

$webProject = Join-Path $v2Root 'src\Web\GiveAID.V2.Web.csproj'
$webDll     = Join-Path $v2Root 'src\Web\bin\Debug\net10.0\GiveAID.V2.Web.dll'
$logFile    = Join-Path $PSScriptRoot 'admin.log'
$pidFile    = Join-Path $PSScriptRoot 'admin.pid'
$adminPort  = 5069

Write-Host "[start-admin] v2 root: $v2Root" -ForegroundColor Cyan

# Pre-flight
Write-Host "[start-admin] Checking port $adminPort..." -ForegroundColor Cyan
Get-NetTCPConnection -LocalPort $adminPort -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    $proc = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "[start-admin] Port $adminPort held by $($proc.ProcessName) (PID $($proc.Id)) - killing" -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
}
Start-Sleep -Seconds 1

if (Test-Path $pidFile) {
    $oldPid = Get-Content $pidFile -ErrorAction SilentlyContinue
    if ($oldPid) {
        try {
            Stop-Process -Id $oldPid -Force -ErrorAction SilentlyContinue
        } catch {}
    }
    Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
}

$dotnet = Get-Command dotnet -ErrorAction SilentlyContinue
if (-not $dotnet) {
    Write-Host "[start-admin] ERROR: dotnet CLI not found" -ForegroundColor Red
    exit 1
}

# Build if needed
$needsBuild = $false
if (-not (Test-Path $webDll)) {
    $needsBuild = $true
} else {
    $csFiles = Get-ChildItem $v2Root -Recurse -Filter '*.cs' -ErrorAction SilentlyContinue |
               Where-Object { $_.FullName -notmatch '\\(bin|obj)\\' -and $_.FullName -match '\\(src\\Web)\\' }
    $dllTime = (Get-Item $webDll).LastWriteTime
    foreach ($f in $csFiles) {
        if ($f.LastWriteTime -gt $dllTime) { $needsBuild = $true; break }
    }
}

if ($needsBuild) {
    Write-Host "[start-admin] Building GiveAID.V2.Web..." -ForegroundColor Cyan
    & dotnet build $webProject -c Debug --nologo -v minimal 2>&1 | Tee-Object -FilePath $logFile -Append
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[start-admin] BUILD FAILED. Check $logFile" -ForegroundColor Red
        exit 1
    }
}

# Start
Write-Host "[start-admin] Starting Admin console on http://localhost:$adminPort..." -ForegroundColor Cyan
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName               = $dotnet.Source
$psi.Arguments              = "run --project `"$webProject`" --no-build --urls http://localhost:$adminPort"
$psi.UseShellExecute        = $false
$psi.WorkingDirectory       = $v2Root

if ($env:ASPNETCORE_ENVIRONMENT) {
    $psi.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = $env:ASPNETCORE_ENVIRONMENT
} else {
    $psi.EnvironmentVariables['ASPNETCORE_ENVIRONMENT'] = 'Development'
}

$proc = [System.Diagnostics.Process]::Start($psi)
Set-Content -Path $pidFile -Value $proc.Id
Write-Host "[start-admin] Admin started, PID $($proc.Id)" -ForegroundColor Green
Write-Host "[start-admin] URL: http://localhost:$adminPort/Admin/Auth/Login" -ForegroundColor Green

# Wait for health
$ok = $false
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds 1
    try {
        $r = Invoke-WebRequest "http://localhost:$adminPort/Admin/Auth/Login" -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($r.StatusCode -eq 200) {
            $ok = $true
            Write-Host "[start-admin] HEALTHY after ${i}s" -ForegroundColor Green
            break
        }
    } catch {}
}
if (-not $ok) {
    Write-Host "[start-admin] WARNING: admin not responding after 60s. Check $logFile" -ForegroundColor Yellow
}

$proc.WaitForExit()
exit $proc.ExitCode
