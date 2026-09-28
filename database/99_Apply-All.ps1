# =====================================================================
# 99_Apply-All.ps1 — Run all database scripts for GiveAID V2
# Idempotent: safe to re-run. Drops + recreates all tables.
#
# Connection string standard: (localdb)\MSSQLLocalDB
#   (matches src/WebApi/appsettings.Development.json)
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File 99_Apply-All.ps1
#   powershell -ExecutionPolicy Bypass -File 99_Apply-All.ps1 -Server "MYHOST\SQLEXPRESS" -Database GiveAIDDB
#
# Output:
#   - Drop+recreate database GiveAIDDB
#   - Apply schema + seed from 01_CreateDatabase_V2.sql
#   - Apply additional seeds from ./seeds/*.ps1 (idempotent)
# =====================================================================

[CmdletBinding()]
param(
    [string]$Server = '(localdb)\MSSQLLocalDB',
    [string]$Database = 'GiveAIDDB',
    [string]$ScriptDir,
    [switch]$SkipSeeds = $false,
    [switch]$VerboseSql = $false
)

# PSScriptRoot is the directory containing this script (resolves correctly
# when invoked via -File). Default the parameter to it when not provided.
if (-not $ScriptDir) {
    $ScriptDir = $PSScriptRoot
}

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

Write-Host '====================================================================' -ForegroundColor Cyan
Write-Host ' GiveAID V2 — Database apply script' -ForegroundColor Cyan
Write-Host " Server   : $Server"                                              -ForegroundColor Cyan
Write-Host " Database : $Database"                                            -ForegroundColor Cyan
Write-Host '====================================================================' -ForegroundColor Cyan

function Invoke-SqlFile {
    param(
        [string]$Path,
        [string]$Db,
        [int]$CommandTimeout = 0  # 0 = unlimited
    )

    if (-not (Test-Path $Path)) {
        throw "SQL script not found: $Path"
    }

    Write-Host ''
    Write-Host ">>> Applying: $Path (db=$Db)" -ForegroundColor Yellow
    $absolutePath = (Resolve-Path $Path).Path
    $output = & sqlcmd -S $Server -E -d $Db -i $absolutePath -b -m -1 `
        -h -1 `
        2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host ($output | Out-String) -ForegroundColor Red
        throw "sqlcmd failed with exit code $LASTEXITCODE for $Path"
    }
    if ($VerboseSql) {
        Write-Host ($output | Out-String)
    } else {
        $output | Where-Object { $_ -match '^(>>>|====|.*Created|.*Seeded|.*PRINT|.*Run)' } | ForEach-Object {
            Write-Host "    $_" -ForegroundColor Gray
        }
    }
}

# 1. Ensure database exists (apply schema script, which creates it if missing).
$schemaScript = Join-Path $ScriptDir '01_CreateDatabase_V2.sql'
if (-not (Test-Path $schemaScript)) {
    throw "Schema script not found at $schemaScript"
}

# Apply against master first (so CREATE DATABASE can run), then against
# GiveAIDDB itself to handle the USE + table/view creation.
Write-Host ''
Write-Host '=== Step 1: Apply schema script (against master, idempotent) ===' -ForegroundColor Magenta
Invoke-SqlFile -Path $schemaScript -Db 'master'

# 2. Apply any extra seeds
if (-not $SkipSeeds) {
    $seedsDir = Join-Path $ScriptDir 'seeds'
    if (Test-Path $seedsDir) {
        Write-Host ''
        Write-Host '=== Step 2: Apply seed scripts (PowerShell) ===' -ForegroundColor Magenta
        $seedScripts = Get-ChildItem -Path $seedsDir -Filter '*.ps1' | Sort-Object Name
        foreach ($seed in $seedScripts) {
            Write-Host ''
            Write-Host ">>> Running seed: $($seed.Name)" -ForegroundColor Yellow
            & powershell -ExecutionPolicy Bypass -File $seed.FullName -Server $Server -Database $Database
            if ($LASTEXITCODE -ne 0) {
                throw "Seed script $($seed.Name) failed with exit code $LASTEXITCODE"
            }
        }
    } else {
        Write-Host '   (no seeds directory — skipping)' -ForegroundColor Gray
    }
}

# 3. Verify
Write-Host ''
Write-Host '=== Step 3: Verification ===' -ForegroundColor Magenta
$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $tables = @()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'"
    $tableCount = [int]$cmd.ExecuteScalar()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.VIEWS WHERE TABLE_SCHEMA='dbo'"
    $viewCount = [int]$cmd.ExecuteScalar()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT name FROM sys.tables WHERE SCHEMA_NAME(schema_id)='dbo' ORDER BY name"
    $reader = $cmd.ExecuteReader()
    $tableNames = New-Object System.Collections.ArrayList
    while ($reader.Read()) { [void]$tableNames.Add($reader.GetString(0)) }
    $reader.Close()

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT name FROM sys.views WHERE SCHEMA_NAME(schema_id)='dbo' ORDER BY name"
    $reader = $cmd.ExecuteReader()
    $viewNames = New-Object System.Collections.ArrayList
    while ($reader.Read()) { [void]$viewNames.Add($reader.GetString(0)) }
    $reader.Close()

    Write-Host "  Tables: $tableCount" -ForegroundColor Green
    $tableNames | ForEach-Object { Write-Host "    - $_" -ForegroundColor Gray }
    Write-Host "  Views : $viewCount" -ForegroundColor Green
    $viewNames | ForEach-Object { Write-Host "    - $_" -ForegroundColor Gray }

    # Spot-check counts
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM cms_pages"
    $cmsPages = [int]$cmd.ExecuteScalar()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM causes"
    $causes = [int]$cmd.ExecuteScalar()
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT COUNT(*) FROM faqs"
    $faqs = [int]$cmd.ExecuteScalar()
    Write-Host ''
    Write-Host "  cms_pages : $cmsPages row(s)" -ForegroundColor Green
    Write-Host "  causes    : $causes row(s)" -ForegroundColor Green
    Write-Host "  faqs      : $faqs row(s)" -ForegroundColor Green
} finally {
    $conn.Close()
    $conn.Dispose()
}

Write-Host ''
Write-Host '====================================================================' -ForegroundColor Cyan
Write-Host ' DONE — Database GiveAIDDB is ready on ' $Server -ForegroundColor Cyan
Write-Host '====================================================================' -ForegroundColor Cyan
Write-Host ''
Write-Host 'Next steps:' -ForegroundColor Yellow
Write-Host '  1. Start the API: cd ..\src\WebApi && dotnet run' -ForegroundColor White
Write-Host '  2. The application seeds the "admin" user with a secure BCrypt hash.' -ForegroundColor White
Write-Host '  3. For Development: set ASPNETCORE_ENVIRONMENT=Development. For Production: set ADMIN_PASSWORD env var.' -ForegroundColor White

