# stop-all.ps1 — Kill all GiveAID v2 dev processes (Backend, Frontend, Admin).
# Run with: npm run stop

$ErrorActionPreference = 'SilentlyContinue'
$scriptDir = $PSScriptRoot

Write-Host "[stop-all] Stopping all GiveAID v2 dev processes..." -ForegroundColor Cyan

# Kill by port
$ports = @(3000, 5000, 3001)
foreach ($p in $ports) {
    $conns = Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue
    foreach ($c in $conns) {
        $proc = Get-Process -Id $c.OwningProcess -ErrorAction SilentlyContinue
        if ($proc) {
            Write-Host "[stop-all] Stopping $($proc.ProcessName) on port $p (PID $($proc.Id))" -ForegroundColor Yellow
            Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

# Kill by process name
$processNames = @('node', 'dotnet', 'GiveAID.V2.WebApi')
foreach ($name in $processNames) {
    $procs = Get-Process -Name $name -ErrorAction SilentlyContinue
    foreach ($p in $procs) {
        # Skip system processes
        if ($p.Id -le 4) { continue }
        Write-Host "[stop-all] Stopping process $name (PID $($p.Id))" -ForegroundColor Yellow
        Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue
    }
}

# Clean up PID files
foreach ($pf in @('backend.pid', 'frontend.pid', 'admin.pid')) {
    $full = Join-Path $scriptDir $pf
    if (Test-Path $full) { 
        Remove-Item $full -Force -ErrorAction SilentlyContinue
        Write-Host "[stop-all] Removed $pf" -ForegroundColor Green
    }
}

Start-Sleep -Milliseconds 500
Write-Host "[stop-all] Done." -ForegroundColor Green
