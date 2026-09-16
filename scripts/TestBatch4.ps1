param(
    [string]$Server='.\SQLEXPRESS,62580',
    [string]$Database='GiveAIDDB'
)
Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=15"
$conn.Open()
try {
    $sql = @"
IF NOT EXISTS (SELECT 1 FROM dbo.Organizations WHERE organization_name = N'Sunrise Education Foundation')
BEGIN
    INSERT INTO dbo.Organizations
        (organization_name, organization_type, description, logo_url, website_url,
         contact_email, contact_phone, address, registration_number,
         mission, vision,
         contribution_amount, contribution_type,
         is_active, is_featured, display_order,
         created_at, updated_at)
    VALUES
        (N'Sunrise Education Foundation',
         N'Partner',
         N'A non-profit foundation funding school supplies, scholarships and learning resources for underprivileged children across Vietnam.',
         NULL,
         N'https://www.sunrise-edu.example.org',
         N'partnerships@sunrise-edu.example.org',
         N'+84-28-3899-0101',
         N'15 Ba Huyen Thanh Quan, District 3, Ho Chi Minh City',
         N'PARTNER-2026-001',
         N'Every child deserves the tools to learn, dream and succeed.',
         N'A Vietnam where no child is denied education because of poverty.',
         250000000.00,
         N'Financial',
         1, 1, 1,
         GETDATE(), GETDATE());
END
"@
    $cmd = $conn.CreateCommand()
    $cmd.CommandTimeout = 60
    $cmd.CommandText = $sql
    $rows = $cmd.ExecuteNonQuery()
    Write-Host "OK rows=$rows"
} finally {
    $conn.Close(); $conn.Dispose()
}
