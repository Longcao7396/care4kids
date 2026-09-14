$ErrorActionPreference = 'SilentlyContinue'
$pidFile = Join-Path $PSScriptRoot 'backend.pid'
if (Test-Path $pidFile) {
    $pid = Get-Content $pidFile
    if ($pid) {
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        Write-Host "[stop-backend] Killed backend (PID $pid)" -ForegroundColor Yellow
    }
    Remove-Item $pidFile -Force
}
# Also kill any orphan iisexpress on backend port
Get-Process iisexpress -ErrorAction SilentlyContinue | Where-Object {
    $_.Path -like '*IIS Express*'
} | ForEach-Object {
    Write-Host "[stop-backend] Killing iisexpress PID $($_.Id)" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}
