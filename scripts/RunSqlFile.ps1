param(
    [Parameter(Mandatory=$true)] [string]$SqlFile,
    [Parameter(Mandatory=$true)] [string]$Server,
    [Parameter(Mandatory=$true)] [string]$Database
)

$ErrorActionPreference = 'Stop'

Add-Type -AssemblyName System.Data -ErrorAction Stop

$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=30;Pooling=False;"
$rawSql = Get-Content -LiteralPath $SqlFile -Raw -Encoding UTF8

# Remove USE statements
$rawSql = $rawSql -replace "(?im)^\s*USE\s+\[?[A-Za-z0-9_]+\]?\s*;\s*$", ""

# Split on GO (case-insensitive, full line)
$lines = $rawSql -split "`r?`n"
$batches = @()
$cur = New-Object System.Text.StringBuilder
foreach ($line in $lines) {
    if ($line -match '^\s*GO\s*(--.*)?$') {
        if ($cur.Length -gt 0) {
            $batches += $cur.ToString().Trim()
            [void]$cur.Clear()
        }
    } else {
        [void]$cur.AppendLine($line)
    }
}
if ($cur.Length -gt 0) { $batches += $cur.ToString().Trim() }

Write-Host "==================================================================="
Write-Host "  File:    $SqlFile"
Write-Host "  Server:  $Server"
Write-Host "  Database:$Database"
Write-Host "  Batches: $($batches.Count)"
Write-Host "==================================================================="

$totalRows = 0
$batchNum = 0
foreach ($batch in $batches) {
    if ([string]::IsNullOrWhiteSpace($batch)) { continue }
    $batchNum++
    $batchOneLine = ($batch -replace "`r?`n", " | ")
    if ($batchOneLine.Length -gt 200) { $batchOneLine = $batchOneLine.Substring(0, 200) + "..." }

    Write-Host ""
    Write-Host "--- Batch $batchNum/$($batches.Count): $batchOneLine ---"

    # Each batch opens its own connection so failures don't poison subsequent batches
    $conn = New-Object System.Data.SqlClient.SqlConnection $connStr
    try {
        $conn.Open()
        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 120
        $cmd.CommandText = $batch

        # Capture PRINT messages
        $handler = [System.Data.SqlClient.SqlInfoMessageEventHandler]{
            param($sender, $eventArg)
            Write-Host "[PRINT] $($eventArg.Message.TrimEnd())"
        }
        $conn.Add_InfoMessage($handler)
        try {
            $rows = $cmd.ExecuteNonQuery()
            Write-Host "[OK] Rows affected: $rows"
            $totalRows += [Math]::Max($rows, 0)
        } finally {
            $conn.Remove_InfoMessage($handler) | Out-Null
        }
    } catch {
        Write-Host "[FAIL] $($_.Exception.Message)"
        Write-Host "[SKIP] Continuing with next batch."
    } finally {
        if ($conn.State -eq 'Open') { $conn.Close() }
        $conn.Dispose()
    }
    Start-Sleep -Milliseconds 200
}

Write-Host ""
Write-Host "==================================================================="
Write-Host "  DONE. Total rows affected: $totalRows"
Write-Host "==================================================================="
