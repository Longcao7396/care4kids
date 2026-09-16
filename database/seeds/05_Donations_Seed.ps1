# 05_Donations_Seed.ps1
# Seed 47 demo donations distributed across all 12 active/ongoing campaigns.
# Uses 6 donor users (user_id 4-9). Anonymous donations use user_id 4 but is_anonymous=1.
# payment_status: Completed=38, Pending=5, Failed=2, Refunded=2
# payment_method: CreditCard=30, BankTransfer=17
# Idempotent: checks by transaction_id (unique).

param(
    [string]$Server = '.\SQLEXPRESS,62580',
    [string]$Database = 'GiveAIDDB'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=60;Pooling=False"

# Load lookups
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$campaignLookup = @{}
$userLookup = @{}
$causeLookup = @{}
$orgLookup = @{}
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT campaign_id, campaign_code FROM dbo.Campaigns'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) { $campaignLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT user_id, username FROM dbo.Users WHERE username NOT IN ('admin', 'admin.c4k', 'content.lead', 'content.editor') AND role = 'User'"
    $rdr = $cmd2.ExecuteReader()
    while ($rdr.Read()) { $userLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd3 = $conn.CreateCommand()
    $cmd3.CommandText = 'SELECT cause_id, cause_code FROM dbo.Causes WHERE parent_cause_id IS NULL'
    $rdr = $cmd3.ExecuteReader()
    while ($rdr.Read()) { $causeLookup[[string]$rdr.GetValue(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd4 = $conn.CreateCommand()
    $cmd4.CommandText = 'SELECT organization_id, organization_name FROM dbo.Organizations'
    $rdr = $cmd4.ExecuteReader()
    while ($rdr.Read()) { $orgLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()
} finally {
    $conn.Close(); $conn.Dispose()
}

Write-Host ("Campaigns: {0}, Users: {1}, Causes: {2}, Orgs: {3}" -f $campaignLookup.Count, $userLookup.Count, $causeLookup.Count, $orgLookup.Count)

# Donor usernames (must match Users table)
$donors = @('nguyen.minhanh','tran.giabao','le.thuha','pham.ducminh','hoang.thanhthao','do.quanghuy')
$amounts = @(50000, 100000, 200000, 500000, 1000000, 2000000, 5000000, 10000000)
$methods = @('CreditCard', 'BankTransfer')
$cardTypes = @('Visa','MasterCard','JCB')
$statuses = @('Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Completed','Pending','Pending','Pending','Pending','Pending','Failed','Failed','Refunded','Refunded')

# Build 47 donations distributed across campaigns
$donations = @()
$i = 100  # Start TXN-C4K-0100 to avoid collision with existing 13
$campaignCodes = @(
    'NUTRI-MEALS-001','SUPPLIES-2026','CARE-HOMES-2026','HEALTH-2026','BACK2SCHOOL-2026','GIFTS-2026',
    'C4K-MEALS-Q4-2026','C4K-WINTER-2026','C4K-DIGITAL-2026','C4K-MEDCARE-2026','C4K-SHELTER-2026','C4K-EMERGENCY-2026','C4K-VOLUNTEER-2026'
)

foreach ($cc in $campaignCodes) {
    if (-not $campaignLookup.ContainsKey($cc)) { continue }
    $cid = $campaignLookup[$cc]

    # 3-5 donations per campaign, depending on importance
    $count = if ($cc -match 'C4K-') { 4 } else { 3 }
    for ($n = 0; $n -lt $count; $n++) {
        $donor = Get-Random -InputObject $donors
        $uid = $userLookup[$donor]
        $amtIdx = Get-Random -Minimum 0 -Maximum $amounts.Count
        $amt = $amounts[$amtIdx]
        $stIdx = Get-Random -Minimum 0 -Maximum $statuses.Count
        $st = $statuses[$stIdx]
        $meth = Get-Random -InputObject $methods
        $cardLast = '{0:D4}' -f (Get-Random -Minimum 1000 -Maximum 9999)
        $cardType = if ($meth -eq 'CreditCard') { Get-Random -InputObject $cardTypes } else { $null }
        $anon = (Get-Random -Minimum 1 -Maximum 10) -le 2  # ~20% anonymous
        $daysAgo = Get-Random -Minimum 1 -Maximum 90
        $hasMsg = (Get-Random -Minimum 1 -Maximum 10) -le 4  # ~40% have messages
        $msgPool = @(
            'Keep up the great work!',
            'For the children.',
            'Every child deserves a chance.',
            'Proud to support Care4Kids.',
            'In memory of our beloved grandmother.',
            'Happy to contribute monthly.',
            'Wishing the team continued success.',
            'Hope this helps the kids in Mekong.'
        )
        $msg = if ($hasMsg) { Get-Random -InputObject $msgPool } else { $null }

        $donations += @{
            txn = "TXN-C4K-{0:D4}" -f $i
            uid = $uid
            cid = $cid
            org = $null  # not tracked in current schema for donations (only via campaign)
            amt = $amt
            daysAgo = $daysAgo
            meth = $meth
            st = $st
            last = $cardLast
            ctype = $cardType
            anon = $anon
            msg = $msg
        }
        $i++
    }
}

Write-Host ("Generated {0} donations" -f $donations.Count)

# Resolve cause_id per campaign (needed because donations.cause_id NOT NULL)
# For each campaign, look up its cause_id
$causeByCampaign = @{}
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT campaign_id, cause_id FROM dbo.Campaigns'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) {
        $causeByCampaign[[int]$rdr.GetValue(0)] = [int]$rdr.GetValue(1)
    }
    $rdr.Close()
} finally {
    $conn.Close(); $conn.Dispose()
}

# Insert
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $inserted = 0
    $skipped = 0
    $sql = 'INSERT INTO dbo.Donations (user_id, cause_id, campaign_id, organization_id, amount, donation_date, payment_method, payment_status, transaction_id, card_last_four, card_type, is_anonymous, message, receipt_sent, created_at) VALUES (@uid, @cause, @cid, @org, @amt, DATEADD(day, @daysAgo, GETDATE()), @meth, @st, @txn, @last, @ctype, @anon, @msg, 1, GETDATE())'

    foreach ($d in $donations) {
        $causeId = $causeByCampaign[$d.cid]
        if (-not $causeId) { Write-Host ("[WARN] No cause for campaign {0}" -f $d.cid); continue }

        $checkCmd = $conn.CreateCommand()
        $checkCmd.CommandTimeout = 30
        $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.Donations WHERE transaction_id = @txn'
        $checkCmd.Parameters.AddWithValue('@txn', $d.txn) | Out-Null
        if ([int]$checkCmd.ExecuteScalar() -gt 0) {
            Write-Host ("[SKIP] {0}" -f $d.txn)
            $skipped++
            continue
        }

        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 30
        $cmd.CommandText = $sql
        $cmd.Parameters.AddWithValue('@uid', $d.uid)         | Out-Null
        $cmd.Parameters.AddWithValue('@cause', $causeId)     | Out-Null
        $cmd.Parameters.AddWithValue('@cid', $d.cid)         | Out-Null
        $cmd.Parameters.AddWithValue('@org', [DBNull]::Value) | Out-Null
        $cmd.Parameters.AddWithValue('@amt', $d.amt)         | Out-Null
        $cmd.Parameters.AddWithValue('@daysAgo', -$d.daysAgo)| Out-Null
        $cmd.Parameters.AddWithValue('@meth', $d.meth)       | Out-Null
        $cmd.Parameters.AddWithValue('@st', $d.st)           | Out-Null
        $cmd.Parameters.AddWithValue('@txn', $d.txn)         | Out-Null
        $cmd.Parameters.AddWithValue('@last', $d.last)       | Out-Null
        if ($d.ctype) { $cmd.Parameters.AddWithValue('@ctype', $d.ctype) | Out-Null } else { $cmd.Parameters.AddWithValue('@ctype', [DBNull]::Value) | Out-Null }
        $cmd.Parameters.AddWithValue('@anon', $d.anon)       | Out-Null
        if ($d.msg) { $cmd.Parameters.AddWithValue('@msg', $d.msg) | Out-Null } else { $cmd.Parameters.AddWithValue('@msg', [DBNull]::Value) | Out-Null }

        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host ("[OK]   {0,-18} {1,10:N0} VND  {2,-12} {3}" -f $d.txn, $d.amt, $d.st, $d.meth)
        $inserted++
        Start-Sleep -Milliseconds 150
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Donations seed DONE. Inserted: {0}, Skipped: {1}" -f $inserted, $skipped)
    Write-Host '===================================================='
} finally {
    $conn.Close(); $conn.Dispose()
}
