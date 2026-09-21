# stop-admin.ps1 — Stop the v2 Admin console started by start-admin.ps1
$ErrorActionPreference = 'SilentlyContinue'
$pidFile = Join-Path $PSScriptRoot 'admin.pid'
if (Test-Path $pidFile) {
    $pid = Get-Content $pidFile
    if ($pid) {
        Stop-Process -Id $pid -Force -ErrorAction SilentlyContinue
        Write-Host "[stop-admin] Killed Admin console (PID $pid)" -ForegroundColor Yellow
    }
    Remove-Item $pidFile -Force
}
Get-Process dotnet -ErrorAction SilentlyContinue | Where-Object {
    $_.CommandLine -like '*GiveAID.V2.Web*' -or
    $_.CommandLine -like '*src\Web\GiveAID.V2.Web.csproj*'
} | ForEach-Object {
    Write-Host "[stop-admin] Killing orphan dotnet PID $($_.Id)" -ForegroundColor Yellow
    Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
}
