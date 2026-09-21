# pre-start.ps1 — Clean ports for GiveAID v2 dev stack.
# Frees dev ports (3000 React, 5231 WebApi, 5069 Admin) by killing stale processes.
# Always exits 0.

$ErrorActionPreference = 'SilentlyContinue'
$logFile = Join-Path $PSScriptRoot 'pre-start.log'

function Write-Log([string]$Msg, [string]$Color = 'Cyan') {
    Write-Host "[pre-start] $Msg" -ForegroundColor $Color
    Add-Content -Path $logFile -Value ("[{0}] {1}" -f (Get-Date -Format 'o'), $Msg)
}

Add-Content -Path $logFile -Value ("`n[{0}] pre-start invoked" -f (Get-Date -Format 'o'))
Write-Log "Cleaning stale GiveAID v2 dev processes..."

# v2 ports: React=3000, WebApi=5000, Admin=5069, plus legacy fallback
$ports = @(3000, 3001, 5000, 5069, 5231, 5001)
foreach ($p in $ports) {
    $conns = Get-NetTCPConnection -LocalPort $p -State Listen -ErrorAction SilentlyContinue
    foreach ($c in $conns) {
        $proc = Get-Process -Id $c.OwningProcess -ErrorAction SilentlyContinue
        if ($proc -and $proc.Id -ne $PID) {
            Write-Log "Port $p -> killing $($proc.ProcessName) PID $($proc.Id)" 'Yellow'
            try { Stop-Process -Id $proc.Id -Force -ErrorAction Stop } catch {
                Write-Log "Could not stop PID $($proc.Id): $_" 'Red'
            }
        }
    }
}

# Drop .pid files
foreach ($pf in @('backend.pid', 'frontend.pid', 'admin.pid')) {
    $full = Join-Path $PSScriptRoot $pf
    if (Test-Path $full) { Remove-Item $full -Force -ErrorAction SilentlyContinue }
}

Start-Sleep -Milliseconds 600
Write-Log "Done."
exit 0
