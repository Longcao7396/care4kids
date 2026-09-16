# 04_Campaigns_Seed.ps1
# Seed 8 additional campaigns tied to existing causes and organizations.
# Campaigns status enum: 'Paused', 'Cancelled', 'Completed', 'Ongoing', 'Upcoming', 'Active'
# Idempotent: checks by campaign_code.

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
$orgLookup = @{}
$causeLookup = @{}
$creatorId = 0
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT organization_id, organization_name FROM dbo.Organizations'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) { $orgLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = 'SELECT cause_id, cause_name FROM dbo.Causes WHERE parent_cause_id IS NULL'
    $rdr = $cmd2.ExecuteReader()
    while ($rdr.Read()) { $causeLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd3 = $conn.CreateCommand()
    $cmd3.CommandText = "SELECT user_id FROM dbo.Users WHERE username = 'admin.c4k'"
    $creatorId = [int]$cmd3.ExecuteScalar()
} finally {
    $conn.Close(); $conn.Dispose()
}

if ($creatorId -eq 0) { throw "admin.c4k user not found" }
Write-Host ("Orgs: {0}, Top-level causes: {1}, Creator user_id: {2}" -f $orgLookup.Count, $causeLookup.Count, $creatorId)

# Campaigns: code | cause_name | org_name | name | desc | goal | raised | start(daysAgo) | end(daysFwd) | image | benef | location | status | feat | prog_type | regReq | maxPart | targetBene | budget
$campaigns = @(
    @{code='C4K-MEALS-Q4-2026'; causeId=261; org='Sunrise Education Foundation'; name='Quarterly Nutritious Meal Programme'; desc='Funding for 90,000 balanced meals distributed to children across 12 care homes in Ho Chi Minh City during Q4 2026.'; goal=95000000; raised=63500000; startAgo=40; endFwd=50; img='https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800'; benef=1500; loc='Ho Chi Minh City'; status='Active'; feat=1; pt='Recurring'; rr=0; max=0; tbene=1500; budget=95000000},
    @{code='C4K-WINTER-2026'; causeId=261; org='Hope Foundation'; name='Winter Warmth Food Drive'; desc='Hot meal distribution and warm clothing kits for children in remote northern mountain provinces during winter 2026.'; goal=45000000; raised=18750000; startAgo=25; endFwd=65; img='https://images.unsplash.com/photo-1547573854-74d2a71d0826?w=800'; benef=800; loc='Ha Giang, Lao Cai'; status='Active'; feat=1; pt='Seasonal'; rr=0; max=0; tbene=800; budget=45000000},
    @{code='C4K-DIGITAL-2026'; causeId=260; org='Mekong Delta Youth Trust'; name='Digital Learning for Every Child'; desc='Tablets, internet connectivity and digital literacy curriculum for 30 rural schools in the Mekong Delta.'; goal=180000000; raised=92000000; startAgo=80; endFwd=180; img='https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=800'; benef=3000; loc='Can Tho, An Giang, Dong Thap'; status='Active'; feat=1; pt='MultiYear'; rr=0; max=0; tbene=3000; budget=180000000},
    @{code='C4K-MEDCARE-2026'; causeId=262; org='Pearl Health Alliance'; name='Mobile Health Outreach 2026'; desc='Mobile clinic operations serving 50 remote communes, providing check-ups, medicines and vaccinations throughout 2026.'; goal=120000000; raised=78000000; startAgo=110; endFwd=160; img='https://images.unsplash.com/photo-1576091160550-2173dba999ef?w=800'; benef=5000; loc='Tien Giang, Ben Tre, Long An'; status='Ongoing'; feat=0; pt='MultiYear'; rr=0; max=0; tbene=5000; budget=120000000},
    @{code='C4K-SHELTER-2026'; causeId=268; org='Mekong Logistics Co., Ltd.'; name='Safe Shelter Renovation 2026'; desc='Full renovation of dormitories, kitchens and play areas at 5 partner care homes in the Mekong Delta region.'; goal=250000000; raised=145000000; startAgo=70; endFwd=120; img='https://images.unsplash.com/photo-1581094794329-c8112a89af12?w=800'; benef=240; loc='Can Tho, Soc Trang'; status='Ongoing'; feat=1; pt='MultiYear'; rr=0; max=0; tbene=240; budget=250000000},
    @{code='C4K-EMERGENCY-2026'; causeId=267; org='Bright Path Children NGO'; name='Emergency Children Aid Fund'; desc='Rapid-response fund providing immediate food, shelter and medical care for children affected by floods and storms in central Vietnam.'; goal=65000000; raised=24500000; startAgo=15; endFwd=150; img='https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?w=800'; benef=600; loc='Central Vietnam'; status='Active'; feat=0; pt='Emergency'; rr=0; max=0; tbene=600; budget=65000000},
    @{code='C4K-VOLUNTEER-2026'; causeId=260; org='Children First Donor Circle'; name='Volunteer Mobilization 2026'; desc='Recruitment, training and deployment of 200+ community volunteers for year-round Care4Kids programmes.'; goal=28000000; raised=8400000; startAgo=20; endFwd=160; img='https://images.unsplash.com/photo-1559027615-cd4628902d4a?w=800'; benef=200; loc='Ho Chi Minh City, Hanoi'; status='Active'; feat=0; pt='Recurring'; rr=1; max=300; tbene=200; budget=28000000},
    @{code='C4K-SPRING-2027'; causeId=260; org='Sunrise Education Foundation'; name='Spring Education Boost 2027'; desc='Pre-planned fundraising campaign for the spring 2027 education kit distribution (registration opening Q1 2027).'; goal=120000000; raised=0; startAgo=-90; endFwd=180; img='https://images.unsplash.com/photo-1497486751825-1233686d5d80?w=800'; benef=1500; loc='Multiple provinces'; status='Upcoming'; feat=1; pt='Seasonal'; rr=0; max=0; tbene=1500; budget=120000000}
)

$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $inserted = 0
    $skipped = 0
    $sql = 'INSERT INTO dbo.Campaigns (cause_id, organization_id, campaign_name, campaign_code, description, goal_amount, raised_amount, start_date, end_date, image_url, beneficiaries_count, location, status, is_featured, display_order, programme_type, registration_required, max_participants, target_beneficiaries, expected_budget, actual_budget, created_at, updated_at, created_by) VALUES (@cause, @org, @name, @code, @desc, @goal, @raised, DATEADD(day, @startAgo, CAST(GETDATE() AS DATE)), DATEADD(day, @endFwd, CAST(GETDATE() AS DATE)), @img, @benef, @loc, @status, @feat, @order, @pt, @rr, @max, @tbene, @ebudget, @abudget, GETDATE(), GETDATE(), @creator)'

    $i = 0
    foreach ($c in $campaigns) {
        $i++
        if (-not $orgLookup.ContainsKey($c.org)) { Write-Host ("[WARN] Org not found: {0}" -f $c.org); continue }
        $orgId = $orgLookup[$c.org]

        $checkCmd = $conn.CreateCommand()
        $checkCmd.CommandTimeout = 30
        $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.Campaigns WHERE campaign_code = @code'
        $checkCmd.Parameters.AddWithValue('@code', $c.code) | Out-Null
        $exists = [int]$checkCmd.ExecuteScalar()
        if ($exists -gt 0) {
            Write-Host ("[SKIP] {0,-26} {1}" -f $c.code, $c.name)
            $skipped++
            continue
        }

        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 30
        $cmd.CommandText = $sql
        $cmd.Parameters.AddWithValue('@cause', $c.causeId) | Out-Null
        $cmd.Parameters.AddWithValue('@org', $orgId)     | Out-Null
        $cmd.Parameters.AddWithValue('@name', $c.name)    | Out-Null
        $cmd.Parameters.AddWithValue('@code', $c.code)    | Out-Null
        $cmd.Parameters.AddWithValue('@desc', $c.desc)    | Out-Null
        $cmd.Parameters.AddWithValue('@goal', $c.goal)    | Out-Null
        $cmd.Parameters.AddWithValue('@raised', $c.raised)| Out-Null
        $cmd.Parameters.AddWithValue('@startAgo', -$c.startAgo) | Out-Null
        $cmd.Parameters.AddWithValue('@endFwd', $c.endFwd)| Out-Null
        $cmd.Parameters.AddWithValue('@img', $c.img)      | Out-Null
        $cmd.Parameters.AddWithValue('@benef', $c.benef)  | Out-Null
        $cmd.Parameters.AddWithValue('@loc', $c.loc)      | Out-Null
        $cmd.Parameters.AddWithValue('@status', $c.status)| Out-Null
        $cmd.Parameters.AddWithValue('@feat', $c.feat)    | Out-Null
        $cmd.Parameters.AddWithValue('@order', $i + 100)  | Out-Null
        $cmd.Parameters.AddWithValue('@pt', $c.pt)        | Out-Null
        $cmd.Parameters.AddWithValue('@rr', $c.rr)        | Out-Null
        if ($c.max -gt 0) { $cmd.Parameters.AddWithValue('@max', $c.max) | Out-Null } else { $cmd.Parameters.AddWithValue('@max', [DBNull]::Value) | Out-Null }
        $cmd.Parameters.AddWithValue('@tbene', $c.tbene)  | Out-Null
        $cmd.Parameters.AddWithValue('@ebudget', $c.budget)| Out-Null
        if ($c.raised -gt 0) { $cmd.Parameters.AddWithValue('@abudget', $c.raised) | Out-Null } else { $cmd.Parameters.AddWithValue('@abudget', [DBNull]::Value) | Out-Null }
        $cmd.Parameters.AddWithValue('@creator', $creatorId)| Out-Null

        $rows = $cmd.ExecuteNonQuery()
        Write-Host ("[OK]   {0,-26} {1,-40} {2}" -f $c.code, $c.name, $c.status)
        $inserted++
        Start-Sleep -Milliseconds 250
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Campaigns seed DONE. Inserted: {0}, Skipped: {1}" -f $inserted, $skipped)
    Write-Host '===================================================='
} finally {
    $conn.Close(); $conn.Dispose()
}
