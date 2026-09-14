# Starts the React dev server.
#   - Pre-flights port 3000 and kills any stale process holding it.
#   - Auto-selects a free port (BROWSER=ignore + PORT fallback) if 3000 is taken.
#   - Forwards all react-scripts CLI args, e.g.   npm start -- --port 3001
# Logs to scripts/frontend.log

$ErrorActionPreference = 'SilentlyContinue'
$logFile    = Join-Path $PSScriptRoot 'frontend.log'
$projectDir = Resolve-Path (Join-Path $PSScriptRoot '..')

Add-Content -Path $logFile -Value ("[{0}] start-frontend called, args: {1}" -f (Get-Date -Format 'o'), ($args -join ' '))

# Determine desired port: --port N flag, or default 3000
$desiredPort = 3000
for ($i = 0; $i -lt $args.Count; $i++) {
    if ($args[$i] -eq '--port' -and ($i + 1) -lt $args.Count) {
        $desiredPort = [int]$args[$i + 1]
        break
    }
}

# --- Pre-flight: free port 3000 if held by stale process ---
Write-Host "[start-frontend] Checking port $desiredPort..." -ForegroundColor Cyan
$listeners = Get-NetTCPConnection -LocalPort $desiredPort -State Listen -ErrorAction SilentlyContinue
foreach ($conn in $listeners) {
    $proc = Get-Process -Id $conn.OwningProcess -ErrorAction SilentlyContinue
    if ($proc) {
        Write-Host "[start-frontend] Port $desiredPort held by $($proc.ProcessName) (PID $($proc.Id)) - killing" -ForegroundColor Yellow
        Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    }
}
Start-Sleep -Seconds 1

# Re-check
$stillBusy = Get-NetTCPConnection -LocalPort $desiredPort -State Listen -ErrorAction SilentlyContinue
if ($stillBusy) {
    Write-Host "[start-frontend] Port $desiredPort STILL busy after kill. Falling back to BROWSER=none and letting react-scripts pick another port." -ForegroundColor Yellow
    $env:BROWSER = 'none'
    $env:PORT = '3001'  # safe fallback
}

# Run react-scripts via node directly so we keep proper signal forwarding
$reactScripts = Join-Path $projectDir 'node_modules\react-scripts\scripts\start.js'
if (-not (Test-Path $reactScripts)) {
    Write-Host "[start-frontend] react-scripts not found at $reactScripts" -ForegroundColor Red
    Write-Host "[start-frontend] Did you forget to run 'npm install' in $projectDir?" -ForegroundColor Yellow
    exit 1
}

Write-Host "[start-frontend] Starting React dev server on port $desiredPort..." -ForegroundColor Cyan
$env:BROWSER = if ($env:BROWSER) { $env:BROWSER } else { 'none' }
$psi = New-Object System.Diagnostics.ProcessStartInfo
$psi.FileName = (Get-Command node).Source
$psi.Arguments = "`"$reactScripts`" $($args -join ' ')"
$psi.WorkingDirectory = $projectDir
$psi.UseShellExecute = $false
$psi.RedirectStandardOutput = $false
$psi.RedirectStandardError = $false

$proc = [System.Diagnostics.Process]::Start($psi)
Write-Host "[start-frontend] react-scripts PID $($proc.Id)" -ForegroundColor Green
Write-Host "[start-frontend] Logging to $logFile (use Ctrl+C to stop)" -ForegroundColor Green

# Wait until react-scripts exits (forward its exit code)
$proc.WaitForExit()
exit $proc.ExitCode
