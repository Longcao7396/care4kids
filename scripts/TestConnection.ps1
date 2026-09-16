param([string]$Server='.\SQLEXPRESS,62580', [string]$Database='GiveAIDDB')
Add-Type -AssemblyName System.Data
$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=60;Pooling=False"
$sw = [System.Diagnostics.Stopwatch]::StartNew()
try {
    $conn = New-Object System.Data.SqlClient.SqlConnection $connStr
    $sw.Stop()
    Write-Host "Created in $($sw.ElapsedMilliseconds)ms"
    $sw.Restart()
    $conn.Open()
    $sw.Stop()
    Write-Host "Opened in $($sw.ElapsedMilliseconds)ms"
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT @@VERSION"
    $ver = $cmd.ExecuteScalar()
    Write-Host "Version: $ver"
    $conn.Close()
} catch {
    Write-Host "ERR: $($_.Exception.Message)"
}
