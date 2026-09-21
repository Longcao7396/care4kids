# stop-backend.ps1 — Stop the v2 WebApi started by start-backend.ps1
$ErrorActionPreference = 'SilentlyContinue'
$pidFile = Join-Path $PSScriptRoot 'backend.pid'
if (Test-Path $pidFile) {
    $pid = Get-Content $pidFile
    if ($pid) {
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        Write-Host "[stop-backend] Killed WebApi (PID $pid)" -ForegroundColor Yellow
    }
    Remove-Item $pidFile -Force
}
# Also kill orphan dotnet hosting the WebApi project (by command line match)
Get-Process dotnet -ErrorAction SilentlyContinue | Where-Object {
    $_.CommandLine -like '*GiveAID.V2.WebApi*' -or
    $_.CommandLine -like '*src\WebApi\GiveAID.V2.WebApi.csproj*'
} | ForEach-Object {
    Write-Host "[stop-backend] Killing orphan dotnet PID $($_.Id)" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}
