param([string]$Query, [string]$Server='localhost,62580', [string]$Database='GiveAIDDB')
Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=60;Pooling=False"
$conn.Open()
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandTimeout = 60
    $cmd.CommandText = $Query
    $rdr = $cmd.ExecuteReader()
    $cols = @()
    for ($i=0; $i -lt $rdr.FieldCount; $i++) { $cols += $rdr.GetName($i) }
    Write-Host ("{0}" -f ($cols -join " | "))
    Write-Host ("-" * 80)
    $count = 0
    while ($rdr.Read()) {
        $vals = @()
        for ($i=0; $i -lt $rdr.FieldCount; $i++) {
            $v = $rdr.GetValue($i)
            if ($v -is [DBNull]) { $vals += "<NULL>" }
            else { $vals += ([string]$v) }
        }
        Write-Host ($vals -join " | ")
        $count++
    }
    $rdr.Close()
    Write-Host ("-" * 80)
    Write-Host ("Total rows: $count")
} finally {
    $conn.Close(); $conn.Dispose()
}
