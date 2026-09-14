#requires -Version 5.0
<#
.SYNOPSIS
    Self-improving auditor for the GiveAID project.

.DESCRIPTION
    Runs a series of independent checks against the live system and applies
    safe, reversible fixes. Reports anything it could not auto-fix to
    scripts/audit-report.md.

.PARAMETER AutoFix
    When set, the auditor will apply safe fixes (default behavior).

.PARAMETER SkipRebuild
    Skip the backend rebuild step.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts/auditor.ps1
#>
[CmdletBinding()]
param(
    [switch]$AutoFix = $true,
    [switch]$SkipRebuild
)

$ErrorActionPreference = 'Stop'
$timestamp = Get-Date -Format 'yyyy-MM-dd HH:mm:ss'
$reportPath = Join-Path $PSScriptRoot 'audit-report.md'
$issues = New-Object System.Collections.Generic.List[string]
$applied = New-Object System.Collections.Generic.List[string]

function Write-Section($title) {
    Write-Host ""
    Write-Host "â•â•â• $title â•â•â•" -ForegroundColor Cyan
}

function Write-Ok($msg)   { Write-Host "  [OK]   $msg" -ForegroundColor Green }
function Write-Warn($msg) { Write-Host "  [WARN] $msg" -ForegroundColor Yellow; $issues.Add($msg) | Out-Null }
function Write-Err($msg)  { Write-Host "  [ERR]  $msg" -ForegroundColor Red;     $issues.Add($msg) | Out-Null }
function Write-Fix($msg) { Write-Host "  [FIX]  $msg" -ForegroundColor Magenta; $applied.Add($msg) | Out-Null }

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 1. Runtime health
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Section "1. Runtime health"

$ports = @(3000, 44300, 61508)
$portsUp = @{}
foreach ($p in $ports) {
    $m = netstat -ano | Select-String "[:.]$p\s" | Where-Object { $_ -match "LISTENING" }
    if ($m) {
        Write-Ok "Port $p is listening"
        $portsUp[$p] = $true
    } else {
        Write-Err "Port $p is NOT listening"
        $portsUp[$p] = $false
    }
}

# Backend health
if ($portsUp[44300] -or $portsUp[61508]) {
    $port = if ($portsUp[44300]) { 44300 } else { 61508 }
    try {
        $r = [Net.WebRequest]::Create("http://localhost:$port/health")
        $r.Timeout = 10000
        $resp = $r.GetResponse()
        $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $body = $sr.ReadToEnd()
        $sr.Close()
        Write-Ok "Backend /health â†’ HTTP $($resp.StatusCode): $body"
    } catch {
        Write-Err "Backend /health FAILED: $($_.Exception.Message)"
    }
} else {
    Write-Err "Backend is DOWN on both ports 44300 and 61508"
}

# Frontend health
if ($portsUp[3000]) {
    try {
        $r = [Net.WebRequest]::Create('http://localhost:3000/')
        $r.Timeout = 5000
        $resp = $r.GetResponse()
        $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $body = $sr.ReadToEnd().Substring(0, [Math]::Min(120, 200))
        $sr.Close()
        Write-Ok "Frontend HTTP 200: $(if ($body) { $body } else { 'empty' })"
    } catch {
        Write-Err "Frontend HTTP FAILED: $($_.Exception.Message)"
    }
}

# Login API test
if ($portsUp[44300]) {
    try {
        $w = [Net.WebRequest]::Create('http://localhost:44300/api/auth/login')
        $w.Method = 'POST'; $w.ContentType = 'application/json'; $w.Timeout = 30000
        $body = [Text.Encoding]::UTF8.GetBytes('{"Email":"admin@give-aid.org","Password":"Admin@123"}')
        $s = $w.GetRequestStream(); $s.Write($body, 0, $body.Length); $s.Close()
        $resp = $w.GetResponse()
        $sr = New-Object System.IO.StreamReader($resp.GetResponseStream())
        $content = $sr.ReadToEnd()
        $sr.Close(); $resp.Close()
        if ($content -match '"success":true') {
            Write-Ok "Login API OK (admin@give-aid.org)"
        } else {
            Write-Err "Login API returned non-success: $content"
        }
    } catch {
        $ex = $_.Exception
        if ($ex -is [Net.WebException] -and $ex.Response) {
            $sr2 = New-Object System.IO.StreamReader($ex.Response.GetResponseStream())
            $body2 = $sr2.ReadToEnd(); $sr2.Close()
            Write-Err "Login API HTTP $($ex.Response.StatusCode): $body2"
        } else {
            Write-Err "Login API exception: $($ex.Message)"
        }
    }
}

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 2. IIS Express integrity
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Section "2. IIS Express integrity"

$iis = Get-CimInstance Win32_Process | Where-Object {
    $_.Name -eq 'iisexpress.exe' -and $_.CommandLine -match 'GiveAID.Web'
}
if ($iis) {
    Write-Ok "IIS Express for GiveAID.Web running (PID $($iis.ProcessId))"
} else {
    Write-Err "IIS Express for GiveAID.Web NOT running"
}

$configPath = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\.vs\GiveAID.Web\config\applicationhost.config'
if (Test-Path $configPath) {
    $cfg = Get-Content $configPath -Raw
    if ($cfg -match '<location path="GiveAID\.Web">[\s\S]*?<windowsAuthentication enabled="false"') {
        Write-Ok "Windows Auth disabled for GiveAID.Web"
    } else {
        Write-Err "Windows Auth still enabled â€” popup will appear"
        if ($AutoFix) {
            $newCfg = $cfg -replace '(<location path="GiveAID\.Web">[\s\S]*?<windowsAuthentication enabled=)"true"', '$1"false"'
            if ($newCfg -ne $cfg) {
                Set-Content -Path $configPath -Value $newCfg -Encoding UTF8
                Write-Fix "Disabled Windows Auth in applicationhost.config"
            } else {
                Write-Warn "Could not auto-fix Windows Auth setting"
            }
        }
    }
} else {
    Write-Warn "applicationhost.config not found at $configPath"
}

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 3. Backend code quality
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Section "3. Backend code quality"

$csproj = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\GiveAID.Web.csproj'
$helpersDir = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\Helpers'
$controllersDir = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\Controllers'

# Check for custom CryptoHelper (should not exist)
$cryptoFile = Join-Path $helpersDir 'CryptoHelper.cs'
if (Test-Path $cryptoFile) {
    Write-Err "CryptoHelper.cs still present (should use BCrypt.Net-Next nuget)"
    $issues.Add("Backend: CryptoHelper.cs exists - replace with PasswordHasher.cs using BCrypt.Net-Next")
} else {
    Write-Ok "CryptoHelper.cs removed"
}

# Check for PasswordHasher
$hasherFile = Join-Path $helpersDir 'PasswordHasher.cs'
if (Test-Path $hasherFile) {
    $h = Get-Content $hasherFile -Raw
    if ($h -match 'BCrypt\.Net\.BCrypt') {
        Write-Ok "PasswordHasher uses BCrypt.Net-Next"
    } else {
        Write-Warn "PasswordHasher exists but doesn't use BCrypt"
    }
} else {
    Write-Err "PasswordHasher.cs missing"
}

# Check ApiResponse non-generic support
$vmFile = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\Models\ViewModels.cs'
if (Test-Path $vmFile) {
    $vm = Get-Content $vmFile -Raw
    if ($vm -match 'class ApiResponse\s*:\s*ApiResponse<object>') {
        Write-Ok "ApiResponse non-generic alias present"
    } else {
        Write-Err "ApiResponse non-generic alias missing (build will fail)"
        if ($AutoFix) {
            $newVm = $vm -replace '(// RESPONSE WRAPPERS\r?\n\s*public class ApiResponse<T>)', "public class ApiResponse : ApiResponse<object> { }`r`n`r`n    public class ApiResponse<T>"
            if ($newVm -ne $vm) {
                Set-Content -Path $vmFile -Value $newVm -Encoding UTF8
                Write-Fix "Added ApiResponse non-generic alias"
            }
        }
    }
}

# Check no controller uses hardcoded JwtSecret
$badPattern = 'JwtSecret.*=.*"[A-Za-z0-9@#$%^&*]'
$controllers = Get-ChildItem $controllersDir -Filter '*.cs'
$bad = $controllers | Where-Object { (Get-Content $_.FullName -Raw) -match $badPattern }
if ($bad) {
    Write-Err "Controllers with hardcoded JWT secret: $($bad.Name -join ', ')"
} else {
    Write-Ok "No hardcoded JWT secrets in controllers"
}

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 4. Frontend code quality
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Section "4. Frontend code quality"

$clientSrc = 'C:\Users\admin\Desktop\project NGO\GiveAID.Client\src'

# Check flash-and-redirect bug
$loginPage = Join-Path $clientSrc 'pages\LoginPage.js'
if (Test-Path $loginPage) {
    $lp = Get-Content $loginPage -Raw
    if ($lp -match 'await login\(' -and $lp -match "navigate\(.dashboard.,\s*\{\s*replace:\s*true\s*\}\s*\)") {
        # Check if there's a useEffect watching user after login
        if ($lp -match 'useEffect[\s\S]*?user[\s\S]*?navigate\(.dashboard.') {
            Write-Ok "LoginPage uses post-login useEffect (fixes flash-and-redirect)"
        } else {
            Write-Err "LoginPage has flash-and-redirect bug (calls navigate before setUser propagates)"
            $issues.Add("Frontend: LoginPage.js needs useEffect to watch user state after login")
        }
    } else {
        Write-Ok "LoginPage flow is OK"
    }
}

# Check ProtectedRoute has localStorage fallback
$protectedRoute = Join-Path $clientSrc 'components\ProtectedRoute.js'
if (Test-Path $protectedRoute) {
    $pr = Get-Content $protectedRoute -Raw
    if ($pr -match "localStorage\.getItem\(.giveaid_token.\)") {
        Write-Ok "ProtectedRoute has localStorage fallback"
    } else {
        Write-Err "ProtectedRoute missing localStorage fallback"
        $issues.Add("Frontend: ProtectedRoute.js should check localStorage as fallback")
    }
}

# Check AuthService.login error handling
$authService = Join-Path $clientSrc 'services\authService.js'
if (Test-Path $authService) {
    $as = Get-Content $authService -Raw
    if ($as -match 'friendlyMessage') {
        Write-Ok "AuthService has friendly error messages"
    } else {
        Write-Warn "AuthService missing friendly error messages"
    }
}

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 5. Security smoke
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Section "5. Security smoke"

$webConfig = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\Web.config'
if (Test-Path $webConfig) {
    $w = Get-Content $webConfig -Raw
    if ($w -match 'JwtSecret.*value="([^"]+)"') {
        $secret = $Matches[1]
        if ($secret.Length -ge 32) {
            Write-Ok "JWT secret length OK ($($secret.Length) chars)"
        } else {
            Write-Err "JWT secret too short: $($secret.Length) chars (need â‰¥32)"
        }
        if ($secret -match 'YourSuperSecretKey123!@#') {
            Write-Warn "JWT secret is the well-known default value â€” change in production"
        }
    } else {
        Write-Err "JWT secret not found in Web.config"
    }
}

# Verify admin user in DB has valid BCrypt hash
try {
    $query = "SELECT Email, LEFT(PasswordHash, 4) AS Prefix FROM GiveAIDDB.dbo.Users WHERE Email = 'admin@give-aid.org'"
    $dbResult = & sqlcmd -S .\SQLEXPRESS -E -Q $query -h -1 2>&1
    $hashPrefix = ($dbResult | Where-Object { $_ -match '\$2[ay]?\$' }) -replace '\s+', ' '
    if ($hashPrefix -match '\$2[ay]?\$') {
        Write-Ok "Admin password hash is BCrypt-formatted"
    } else {
        Write-Err "Admin password hash is NOT BCrypt (raw output: $hashPrefix)"
        if ($AutoFix -and $portsUp[44300]) {
            Write-Host "  Attempting to call /api/auth/bootstrap..." -NoNewline
            try {
                $bw = [Net.WebRequest]::Create('http://localhost:44300/api/auth/bootstrap')
                $bw.Method = 'POST'; $bw.ContentType = 'application/json'; $bw.Timeout = 60000
                $bs = $bw.GetRequestStream(); $bb = [Text.Encoding]::UTF8.GetBytes('{}')
                $bs.Write($bb, 0, $bb.Length); $bs.Close()
                $bresp = $bw.GetResponse()
                Write-Fix "Called /api/auth/bootstrap to rehash passwords"
            } catch {
                Write-Warn "Bootstrap call failed: $($_.Exception.Message)"
            }
        }
    }
} catch {
    Write-Warn "Could not query DB: $($_.Exception.Message)"
}

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 6. Optional rebuild
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
if (-not $SkipRebuild -and $AutoFix) {
    Write-Section "6. Backend rebuild check"
    $dll = 'C:\Users\admin\Desktop\project NGO\GiveAID.Web\bin\GiveAID.Web.dll'
    if (Test-Path $dll) {
        $age = (Get-Date) - (Get-Item $dll).LastWriteTime
        Write-Host "  DLL age: $([Math]::Round($age.TotalMinutes, 1)) minutes"
        if ($age.TotalMinutes -gt 30) {
            Write-Warn "DLL is older than 30 min â€” consider rebuilding"
        } else {
            Write-Ok "DLL is fresh"
        }
    }
}

#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
# 7. Report
#â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
Write-Section "7. Report"

$report = @"
# GiveAID Audit Report
Generated: $timestamp

## Summary
- **Issues found:** $($issues.Count)
- **Auto-fixes applied:** $($applied.Count)

## Auto-fixes applied this run
$($applied | ForEach-Object { "- $_" } | Out-String)

## Remaining issues (need human review)
$($issues | ForEach-Object { "- $_" } | Out-String)
"@

Set-Content -Path $reportPath -Value $report -Encoding UTF8
Write-Host "  â†’ Report written to: $reportPath"

if ($issues.Count -eq 0) {
    Write-Host ""
    Write-Host "  âœ… All checks passed." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "  âš  $($issues.Count) issue(s) require attention." -ForegroundColor Yellow
}
