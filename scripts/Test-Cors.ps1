# scripts/Test-Cors.ps1
# ------------------------------------------------------------------
# Runtime CORS integration test for GiveAID v2 WebApi.
#
# Boots the backend in Production mode with a fixed
# Cors:AllowedOrigins list, then verifies:
#   1. An OPTIONS preflight from an ALLOWED origin is answered with
#      the expected CORS response headers (origin echo, credentials,
#      allowed methods/headers).
#   2. A simple GET /healthz from an ALLOWED origin returns the
#      Access-Control-Allow-Origin echo header.
#   3. An OPTIONS preflight from a ROGUE origin does NOT echo the
#      rogue origin back in Access-Control-Allow-Origin.
#
# PowerShell note: Invoke-WebRequest strips some HTTP headers when the
# response body is empty (notably the CORS preflight reply). To get a
# faithful view we use a raw HttpClient (.NET) call so we can compare
# headers byte-for-byte.
# ------------------------------------------------------------------

[CmdletBinding()]
param(
    [int]   $ApiPort        = 5231,
    [int]   $WaitSeconds    = 60,
    [string]$AllowedOrigin  = "http://localhost:3000",
    [string]$AllowedOrigin2 = "https://app.giveaid.org",
    [string]$RogueOrigin    = "https://evil.example.com",
    [string]$TestEndpoint   = "/api/v1/causes"
)

$ErrorActionPreference = 'Stop'
$ProgressPreference    = 'SilentlyContinue'

$root    = Resolve-Path (Join-Path $PSScriptRoot '..')
$proj    = Join-Path $root 'src\WebApi\GiveAID.V2.WebApi.csproj'
$pidFile = Join-Path $PSScriptRoot 'cors-test.pid'
$logFile = Join-Path $PSScriptRoot 'cors-test.log'

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "[FAIL] dotnet CLI not found in PATH" -ForegroundColor Red
    exit 2
}

Remove-Item $logFile, "$logFile.err", $pidFile -Force -ErrorAction SilentlyContinue

Get-NetTCPConnection -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    $p = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
    if ($p) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
}
Start-Sleep -Milliseconds 800

# --- Production-like environment ---
$env:ASPNETCORE_ENVIRONMENT          = "Production"
$env:Cors__AllowedOrigins            = "$AllowedOrigin,$AllowedOrigin2"
$env:Jwt__Secret                     = "CorsTestSecret_$(Get-Random)_MIN32CHARS"
$env:ConnectionStrings__DefaultConnection = "Server=(localdb)\\MSSQLLocalDB;Database=GiveAIDDB_CorsTest;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=5"
$env:SeedData__Enabled               = "false"

Write-Host "[INFO] Starting WebApi in Production mode on port $ApiPort ..." -ForegroundColor Cyan
Write-Host "[INFO] Allowed origins = $($env:Cors__AllowedOrigins)" -ForegroundColor DarkGray

$proc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run --project `"$proj`" --no-build --no-launch-profile --urls http://localhost:$ApiPort" `
    -WorkingDirectory $root `
    -PassThru `
    -RedirectStandardOutput $logFile `
    -RedirectStandardError  "$logFile.err"

Set-Content -Path $pidFile -Value $proc.Id

# --- Wait for /healthz ---
Write-Host "[INFO] Waiting for /healthz (up to $WaitSeconds s)..." -ForegroundColor Cyan
$ready = $false
for ($i = 0; $i -lt $WaitSeconds; $i++) {
    Start-Sleep -Seconds 1
    try {
        $r = Invoke-WebRequest -Uri "http://localhost:$ApiPort/healthz" -UseBasicParsing -TimeoutSec 3 -ErrorAction Stop
        if ($r.StatusCode -eq 200) { $ready = $true; break }
    } catch {}
}
if (-not $ready) {
    Write-Host "[FAIL] Backend did not respond on /healthz within $WaitSeconds s" -ForegroundColor Red
    Write-Host "--- last 40 log lines ---" -ForegroundColor Yellow
    if (Test-Path $logFile) { Get-Content $logFile -Tail 40 }
    Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
    exit 2
}
Write-Host "[OK ] Backend is up (PID $($proc.Id))" -ForegroundColor Green

# --- Helpers ---
$failures = New-Object System.Collections.Generic.List[string]

function Assert-Equal {
    param($Expected, $Actual, $Label)
    if ($Expected -eq $Actual) {
        Write-Host "[OK ] $Label = $Actual" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] $Label expected '$Expected' but got '$Actual'" -ForegroundColor Red
        $failures.Add($Label) | Out-Null
    }
}

function Send-Raw {
    param(
        [string]$Method,
        [string]$Url,
        [hashtable]$Headers
    )
    Add-Type -AssemblyName System.Net.Http -ErrorAction SilentlyContinue
    $client = [System.Net.Http.HttpClient]::new()
    $req = [System.Net.Http.HttpRequestMessage]::new([System.Net.Http.HttpMethod]::new($Method), $Url)
    foreach ($k in $Headers.Keys) {
        $req.Headers.TryAddWithoutValidation($k, $Headers[$k]) | Out-Null
    }
    try {
        $resp = $client.SendAsync($req).GetAwaiter().GetResult()
        $hdrs = @{}
        foreach ($h in $resp.Headers) {
            $hdrs[$h.Key] = ($h.Value -join ', ')
        }
        if ($resp.Content) {
            foreach ($h in $resp.Content.Headers) {
                $hdrs[$h.Key] = ($h.Value -join ', ')
            }
        }
        $body = if ($resp.Content) { $resp.Content.ReadAsStringAsync().GetAwaiter().GetResult() } else { "" }
        return [pscustomobject]@{
            StatusCode = [int]$resp.StatusCode
            Headers    = $hdrs
            Body       = $body
        }
    } finally {
        $client.Dispose()
    }
}

# === TEST 1: Preflight from ALLOWED origin ===
Write-Host ""
Write-Host "=== TEST 1: Preflight from ALLOWED origin ($AllowedOrigin) ===" -ForegroundColor Cyan
$r1 = Send-Raw -Method "OPTIONS" -Url "http://localhost:$ApiPort$TestEndpoint" -Headers @{
    "Origin"                         = $AllowedOrigin
    "Access-Control-Request-Method"  = "GET"
    "Access-Control-Request-Headers" = "authorization,content-type"
}
Write-Host "  StatusCode : $($r1.StatusCode)"
foreach ($k in $r1.Headers.Keys) { Write-Host "  $k : $($r1.Headers[$k])" }
Assert-Equal 204                  $r1.StatusCode                                    "Preflight status code"
Assert-Equal $AllowedOrigin       $r1.Headers["Access-Control-Allow-Origin"]        "Allowed origin echo"
Assert-Equal "true"               $r1.Headers["Access-Control-Allow-Credentials"]   "Allow-Credentials"
if ($r1.Headers["Access-Control-Allow-Methods"]) {
    Write-Host "[OK ] Allow-Methods : $($r1.Headers['Access-Control-Allow-Methods'])" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Allow-Methods header missing" -ForegroundColor Red
    $failures.Add("allow-methods-missing") | Out-Null
}

# === TEST 2: Preflight from ROGUE origin ===
Write-Host ""
Write-Host "=== TEST 2: Preflight from ROGUE origin ($RogueOrigin) must be REJECTED ===" -ForegroundColor Cyan
$r2 = Send-Raw -Method "OPTIONS" -Url "http://localhost:$ApiPort$TestEndpoint" -Headers @{
    "Origin"                         = $RogueOrigin
    "Access-Control-Request-Method"  = "GET"
}
Write-Host "  StatusCode : $($r2.StatusCode)"
foreach ($k in $r2.Headers.Keys) { Write-Host "  $k : $($r2.Headers[$k])" }
$rogueAcao = $r2.Headers["Access-Control-Allow-Origin"]
if ([string]::IsNullOrEmpty($rogueAcao) -or ($rogueAcao -ne $RogueOrigin)) {
    Write-Host "[OK ] Rogue origin was NOT echoed (got '$rogueAcao')" -ForegroundColor Green
} else {
    Write-Host "[FAIL] Rogue origin '$RogueOrigin' was echoed in Access-Control-Allow-Origin" -ForegroundColor Red
    $failures.Add("rogue-origin-echoed") | Out-Null
}

# === TEST 3: Simple GET /healthz from ALLOWED origin ===
Write-Host ""
Write-Host "=== TEST 3: Simple GET /healthz from ALLOWED origin ===" -ForegroundColor Cyan
$r3 = Send-Raw -Method "GET" -Url "http://localhost:$ApiPort/healthz" -Headers @{
    "Origin" = $AllowedOrigin
}
Write-Host "  StatusCode : $($r3.StatusCode)"
foreach ($k in $r3.Headers.Keys) { Write-Host "  $k : $($r3.Headers[$k])" }
Assert-Equal 200                  $r3.StatusCode                                    "GET /healthz status"
Assert-Equal $AllowedOrigin       $r3.Headers["Access-Control-Allow-Origin"]        "Allowed origin echo on GET"
Assert-Equal "true"               $r3.Headers["Access-Control-Allow-Credentials"]   "Allow-Credentials on GET"

# --- Teardown ---
Write-Host ""
Write-Host "[INFO] Stopping backend (PID $($proc.Id))..." -ForegroundColor Cyan
Get-NetTCPConnection -LocalPort $ApiPort -State Listen -ErrorAction SilentlyContinue | ForEach-Object {
    $p = Get-Process -Id $_.OwningProcess -ErrorAction SilentlyContinue
    if ($p) { Stop-Process -Id $p.Id -Force -ErrorAction SilentlyContinue }
}
Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
Remove-Item $pidFile -Force -ErrorAction SilentlyContinue

Write-Host ""
if ($failures.Count -eq 0) {
    Write-Host "=================================================" -ForegroundColor Green
    Write-Host "  ALL CORS CHECKS PASSED" -ForegroundColor Green
    Write-Host "=================================================" -ForegroundColor Green
    exit 0
} else {
    Write-Host "=================================================" -ForegroundColor Red
    Write-Host "  $($failures.Count) CHECK(S) FAILED: $($failures -join ', ')" -ForegroundColor Red
    Write-Host "=================================================" -ForegroundColor Red
    exit 1
}
