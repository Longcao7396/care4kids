# Auto-starts the GiveAID backend via IIS Express.
# Logs to scripts/backend.log and writes PID to scripts/backend.pid

$ErrorActionPreference = 'Stop'
$projectRoot = Join-Path $PSScriptRoot '..'
$backendDir  = Join-Path $projectRoot '..\GiveAID.Web'
$dllPath     = Join-Path $backendDir 'bin\GiveAID.Web.dll'
$slnPath     = Join-Path $backendDir 'GiveAID.Web.sln'
$msbuild     = 'C:\Windows\Microsoft.NET\Framework64\v4.0.30319\MSBuild.exe'
$logFile     = Join-Path $PSScriptRoot 'backend.log'
$pidFile     = Join-Path $PSScriptRoot 'backend.pid'

# --- Pre-flight: free up IIS Express ports if held by stale processes ---
Write-Host "[start-backend] Checking ports 44300 / 61508 / 44301..." -ForegroundColor Cyan
foreach ($p in @(44300, 61508, 44301)) {
    $listeners = Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue
    foreach ($conn in $listeners) {
        $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
        if ($proc) {
            $name = $proc.ProcessName
            Write-Host "[start-backend] Port $p held by $name (PID $($proc.Id)) - killing" -ForegroundColor Yellow
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
    }
}
Start-Sleep -Seconds 1

# Kill any leftover backend from previous run
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

# Build if DLL missing or older than any .cs file
$needsBuild = $false
if (-not (Test-Path $dllPath)) {
    $needsBuild = $true
} else {
    $csFiles = Get-ChildItem $backendDir -Recurse -Filter '*.cs' -ErrorAction SilentlyContinue
    foreach ($f in $csFiles) {
        if ($f.LastWriteTime -gt (Get-Item $dllPath).LastWriteTime) {
            $needsBuild = $true
            break
        }
    }
}

if ($needsBuild) {
    Write-Host "[start-backend] Building GiveAID.Web..." -ForegroundColor Cyan
    & $msbuild $slnPath /p:Configuration=Debug /v:minimal /nologo 2>&1 | Tee-Object -FilePath $logFile -Append
    if ($LASTEXITCODE -ne 0) {
        Write-Host "[start-backend] BUILD FAILED. Check $logFile" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "[start-backend] DLL up-to-date, skipping build" -ForegroundColor Green
}

# Try IIS Express first
$iisExpress = Get-ChildItem 'C:\Program Files\IIS Express\iisexpress.exe' -ErrorAction SilentlyContinue | Select-Object -First 1

if ($iisExpress) {
    Write-Host "[start-backend] Starting IIS Express for site 'GiveAID.Web' on ports 44300 (http), 61508 (http), 44300 (https)" -ForegroundColor Cyan

    # Launch IIS Express using the .NET Process class (bypasses PowerShell's argument parser
    # which incorrectly splits quoted paths on spaces).  We also use the 8.3 short path
    # for any path with spaces as an additional safeguard.
    #
    # Two bugs caused the original /path approach to launch "Development Web Site" instead
    # of "GiveAID.Web":
    #   (a) /path makes IIS Express load its default config, which only has "WebSite1";
    #       we use /site:GiveAID.Web + /userhome pointing to the project's .vs folder.
    #   (b) The Process-scope $env:IIS_USER_HOME from a stale prior shell session is
    #       inherited by .NET Process children, making /config fail.  We .Remove() it.
    #   (c) The project's .vs\GiveAID.Web\config\redirection.config had the wrong XML
    #       section name; it must be "configurationRedirection" (root element), not
    #       "redirectionConfiguration" inside a sectionGroup.

    $projectConfigDir = "$backendDir\.vs\GiveAID.Web"

    # Use 8.3 short path for the project config dir (avoids spaces in the /userhome argument)
    $fso   = New-Object -ComObject Scripting.FileSystemObject
    $userhome = $fso.GetFolder($projectConfigDir).ShortPath

    $psi            = New-Object System.Diagnostics.ProcessStartInfo
    $psi.FileName   = $iisExpress.FullName
    $psi.Arguments  = "/site:GiveAID.Web /userhome:""$userhome"" /clr:4.0"
    $psi.UseShellExecute = $false
    # Remove any stale IIS_USER_HOME so /userhome is not confused with /config
    $null = $psi.EnvironmentVariables.Remove('IIS_USER_HOME')

    $proc = [System.Diagnostics.Process]::Start($psi)
    Set-Content -Path $pidFile -Value $proc.Id
    Write-Host "[start-backend] IIS Express started, PID $($proc.Id), logging to $logFile" -ForegroundColor Green
    Write-Host "[start-backend] Backend URLs: http://localhost:44300  http://localhost:61508" -ForegroundColor Green

    # Wait up to 30s for backend /health to respond
    $ok = $false
    for ($i = 0; $i -lt 30; $i++) {
        Start-Sleep -Seconds 1
        try {
            $r = Invoke-WebRequest 'http://localhost:44300/health' -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
            if ($r.StatusCode -eq 200) {
                $ok = $true
                Write-Host "[start-backend] HEALTHY after ${i}s" -ForegroundColor Green
                break
            }
        } catch {}
    }
    if (-not $ok) {
        Write-Host "[start-backend] WARNING: backend did not become healthy in 30s. Check $logFile" -ForegroundColor Yellow
    }
} else {
    # Fallback: warn user to start backend manually
    Write-Host "[start-backend] IIS Express NOT FOUND at C:\Program Files\IIS Express\" -ForegroundColor Red
    Write-Host "[start-backend] Open Visual Studio -> F5 on GiveAID.Web, or install IIS Express from:" -ForegroundColor Yellow
    Write-Host "[start-backend] https://www.iis.net/downloads/microsoft-iis-express" -ForegroundColor Yellow
    Write-Host "[start-backend] Continuing with frontend only..." -ForegroundColor Yellow
    # Do NOT exit 1 -- frontend can still start, user will see Network Error
    Set-Content -Path $pidFile -Value '' -Force
}
