# 06_Registrations_Seed.ps1
# Seed 40 ProgrammeRegistrations + 10 CampaignRegistrations (total 50).
# Programme status enum: 'Cancelled', 'Attended', 'Confirmed', 'Registered'
# Idempotent: checks (user_id, programme_id) before insert (UNIQUE constraint).

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
$progLookup = @{}
$userList = @()
$campLookup = @{}
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT programme_id, title FROM dbo.Programmes'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) { $progLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = "SELECT user_id FROM dbo.Users WHERE role = 'User' AND is_active = 1"
    $rdr = $cmd2.ExecuteReader()
    while ($rdr.Read()) { $userList += [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd3 = $conn.CreateCommand()
    $cmd3.CommandText = 'SELECT campaign_id, campaign_code FROM dbo.Campaigns'
    $rdr = $cmd3.ExecuteReader()
    while ($rdr.Read()) { $campLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()
} finally {
    $conn.Close(); $conn.Dispose()
}

Write-Host ("Programmes: {0}, Users: {1}, Campaigns: {2}" -f $progLookup.Count, $userList.Count, $campLookup.Count)

$progStatuses = @('Registered','Registered','Registered','Confirmed','Confirmed','Confirmed','Attended','Attended','Cancelled')
$progTitles = @(
    'Saturday Community Kitchen',
    'Weekend Meal Drive',
    'School Lunch Programme',
    'Back-to-School Kit Distribution 2026',
    'Weekend Reading Club',
    'Digital Literacy Workshop',
    'Scholarship Awarding Ceremony 2026',
    'Mobile Health Clinic - Quarter 2',
    'Maternal Care Programme',
    'Vaccination Awareness Day',
    'Warm Winter Clothing Drive',
    'Care Home Renovation Phase 2',
    'Children Day Festival 2026',
    'Volunteer Appreciation Gala'
)

$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$inserted = 0
$skipped = 0
try {
    $sqlProg = 'INSERT INTO dbo.ProgrammeRegistrations (user_id, programme_id, registration_date, status, notes, attendance_confirmed) VALUES (@uid, @pid, DATEADD(day, @daysAgo, GETDATE()), @status, @notes, @att)'

    foreach ($ptitle in $progTitles) {
        if (-not $progLookup.ContainsKey($ptitle)) { continue }
        $progId = $progLookup[$ptitle]

        # 3-4 registrations per programme
        $count = Get-Random -Minimum 3 -Maximum 5
        for ($n = 0; $n -lt $count; $n++) {
            $uid = Get-Random -InputObject $userList
            $st = Get-Random -InputObject $progStatuses
            $daysAgo = Get-Random -Minimum 1 -Maximum 60
            $hasNotes = (Get-Random -Minimum 1 -Maximum 10) -le 3
            $notesPool = @(
                'Bringing my kids along to volunteer.',
                'First time volunteering - excited!',
                'Have 5 years experience with children.',
                'Translator (Vietnamese-English) available.',
                'Can help with photography for the event.',
                'Will bring donated supplies.'
            )
            $notes = if ($hasNotes) { Get-Random -InputObject $notesPool } else { $null }
            $att = if ($st -eq 'Attended') { 1 } else { 0 }

            # Check uniqueness
            $checkCmd = $conn.CreateCommand()
            $checkCmd.CommandTimeout = 120
            $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.ProgrammeRegistrations WHERE user_id = @uid AND programme_id = @pid'
            $checkCmd.Parameters.AddWithValue('@uid', $uid) | Out-Null
            $checkCmd.Parameters.AddWithValue('@pid', $progId) | Out-Null
            if ([int]$checkCmd.ExecuteScalar() -gt 0) {
                $skipped++
                continue
            }

            $cmd = $conn.CreateCommand()
            $cmd.CommandTimeout = 120
            $cmd.CommandText = $sqlProg
            $cmd.Parameters.AddWithValue('@uid', $uid)        | Out-Null
            $cmd.Parameters.AddWithValue('@pid', $progId)        | Out-Null
            $cmd.Parameters.AddWithValue('@daysAgo', -$daysAgo)| Out-Null
            $cmd.Parameters.AddWithValue('@status', $st)      | Out-Null
            if ($notes) { $cmd.Parameters.AddWithValue('@notes', $notes) | Out-Null } else { $cmd.Parameters.AddWithValue('@notes', [DBNull]::Value) | Out-Null }
            $cmd.Parameters.AddWithValue('@att', $att)        | Out-Null

            $cmd.ExecuteNonQuery() | Out-Null
            Write-Host ("[PROG] pid={0,2} uid={1,2} {2,-12} {3}" -f $progId, $uid, $st, $ptitle)
            $inserted++
            Start-Sleep -Milliseconds 150
        }
    }

    # Also seed CampaignRegistrations for campaigns that have registration_required=1
    $campRegStatuses = @('Registered','Registered','Confirmed','Confirmed','Attended','Cancelled')
    $sqlCamp = 'INSERT INTO dbo.CampaignRegistrations (campaign_id, user_id, registration_date, status, notes, attendance_confirmed) VALUES (@cid, @uid, DATEADD(day, @daysAgo, GETDATE()), @status, @notes, @att)'

    foreach ($cc in @('C4K-VOLUNTEER-2026','C4K-WINTER-2026','C4K-MEALS-Q4-2026')) {
        if (-not $campLookup.ContainsKey($cc)) { continue }
        $cid = $campLookup[$cc]
        $count = Get-Random -Minimum 3 -Maximum 5
        for ($n = 0; $n -lt $count; $n++) {
            $uid = Get-Random -InputObject $userList
            $st = Get-Random -InputObject $campRegStatuses
            $daysAgo = Get-Random -Minimum 1 -Maximum 30
            $att = if ($st -eq 'Attended') { 1 } else { 0 }

            $checkCmd = $conn.CreateCommand()
            $checkCmd.CommandTimeout = 120
            $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.CampaignRegistrations WHERE user_id = @uid AND campaign_id = @cid'
            $checkCmd.Parameters.AddWithValue('@uid', $uid) | Out-Null
            $checkCmd.Parameters.AddWithValue('@cid', $cid) | Out-Null
            if ([int]$checkCmd.ExecuteScalar() -gt 0) {
                $skipped++
                continue
            }

            $cmd = $conn.CreateCommand()
            $cmd.CommandTimeout = 120
            $cmd.CommandText = $sqlCamp
            $cmd.Parameters.AddWithValue('@cid', $cid)        | Out-Null
            $cmd.Parameters.AddWithValue('@uid', $uid)        | Out-Null
            $cmd.Parameters.AddWithValue('@daysAgo', -$daysAgo)| Out-Null
            $cmd.Parameters.AddWithValue('@status', $st)      | Out-Null
            $cmd.Parameters.AddWithValue('@notes', [DBNull]::Value) | Out-Null
            $cmd.Parameters.AddWithValue('@att', $att)        | Out-Null

            $cmd.ExecuteNonQuery() | Out-Null
            Write-Host ("[CAMP] cid={0,2} uid={1,2} {2,-12} {3}" -f $cid, $uid, $st, $cc)
            $inserted++
            Start-Sleep -Milliseconds 150
        }
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Registrations seed DONE. Inserted: {0}, Skipped (dup): {1}" -f $inserted, $skipped)
    Write-Host '===================================================='
} finally {
    $conn.Close(); $conn.Dispose()
}
