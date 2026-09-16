# Restart GiveAID.Web on IIS Express with the rotated JWT secret as env var.
$ErrorActionPreference = 'Stop'

# Load the JWT secret. Override GIVEAID_JWT_SECRET in CI/CD to rotate.
if (-not $env:GIVEAID_JWT_SECRET) {
    Write-Host "[start] GIVEAID_JWT_SECRET not set; falling back to web.config" -ForegroundColor Yellow
}

$env:Environment = 'Development'

$root    = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web'
$iisExe  = 'C:\Program Files\IIS Express\iisexpress.exe'
$userhome = (Get-Item "$root\.vs\GiveAID.Web").FullName

if (-not (Test-Path $iisExe)) {
    Write-Host "[start] IIS Express not found at $iisExe" -ForegroundColor Red
    exit 1
}

# Free port 44300 first
Get-NetTCPConnection -LocalPort 44300 -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue
}
Get-Process iisexpress -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1

Set-Location $root
$args = @(
    '/site:GiveAID.Web'
    "/userhome:$userhome"
    '/clr:4.0'
)
Write-Host "[start] Launching: $iisExe $($args -join ' ')" -ForegroundColor Cyan
$proc = Start-Process -FilePath $iisExe -ArgumentList $args -PassThru -RedirectStandardOutput "$root\backend.out.log" -RedirectStandardError "$root\backend.err.log"
Write-Host "[start] PID: $($proc.Id)" -ForegroundColor Green
Start-Sleep -Seconds 8
$ok = $false
for ($i = 0; $i -lt 30; $i++) {
    try {
        $r = Invoke-WebRequest 'http://localhost:44300/api/causes/tree?activeOnly=true' -UseBasicParsing -TimeoutSec 2 -ErrorAction Stop
        if ($r.StatusCode -eq 200) {
            $ok = $true
            Write-Host "[start] HEALTHY after ${i}s" -ForegroundColor Green
            break
        }
    } catch {}
    Start-Sleep -Seconds 1
}
if (-not $ok) {
    Write-Host "[start] WARN: backend did not become healthy in 30s. Logs:" -ForegroundColor Yellow
    Get-Content "$root\backend.out.log" -Tail 20 -ErrorAction SilentlyContinue
    Get-Content "$root\backend.err.log" -Tail 20 -ErrorAction SilentlyContinue
}
