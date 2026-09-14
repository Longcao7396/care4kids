# Auto-kills any node process holding port 3000.
# Logs to scripts/frontend-stop.log

$ErrorActionPreference = 'SilentlyContinue'
$logFile = Join-Path $PSScriptRoot 'frontend-stop.log'

Add-Content -Path $logFile -Value ("[{0}] stop-frontend called" -f (Get-Date -Format 'o'))

$killed = $false
$c = Get-NetTCPConnection -LocalPort 3000 -State Listen -ErrorAction SilentlyContinue
if ($c) {
    foreach ($x in $c) {
        $pid = $x.OwningProcess
        try {
            Stop-Process -Id $pid -Force -ErrorAction Stop
            Add-Content -Path $logFile -Value ("[{0}] killed PID {1} on port 3000" -f (Get-Date -Format 'o'), $pid)
            $killed = $true
        } catch {
            Add-Content -Path $logFile -Value ("[{0}] failed to kill PID {1}: {2}" -f (Get-Date -Format 'o'), $pid, $_.Exception.Message)
        }
    }
}

# Also kill any lingering react-scripts node processes from prior runs
$proc = Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq 'node.exe' -and $_.CommandLine -match 'react-scripts'
}
foreach ($p in $proc) {
    try {
        Stop-Process -Id $p.ProcessId -Force -ErrorAction Stop
        Add-Content -Path $logFile -Value ("[{0}] killed react-scripts node PID {1}" -f (Get-Date -Format 'o'), $p.ProcessId)
        $killed = $true
    } catch {}
}

if ($killed) {
    Write-Host "[stop-frontend] Killed processes on port 3000" -ForegroundColor Yellow
} else {
    Write-Host "[stop-frontend] Port 3000 already free" -ForegroundColor Green
}
