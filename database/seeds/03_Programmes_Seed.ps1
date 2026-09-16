# 03_Programmes_Seed.ps1
# Seed 14 community programmes across Care4Kids's 4 main programme areas.
# Idempotent: checks (title, organization_id) before insert.

param(
    [string]$Server = '.\SQLEXPRESS,62580',
    [string]$Database = 'GiveAIDDB'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=60;Pooling=False"

# --- Phase 1: load lookups ---
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$orgLookup = @{}
$creatorId = 0
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT organization_id, organization_name FROM dbo.Organizations'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) {
        $orgLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0)
    }
    $rdr.Close()

    $userCmd = $conn.CreateCommand()
    $userCmd.CommandText = "SELECT user_id FROM dbo.Users WHERE username = 'admin.c4k'"
    $creatorId = [int]$userCmd.ExecuteScalar()
} finally {
    $conn.Close(); $conn.Dispose()
}

if ($creatorId -eq 0) { throw "admin.c4k user not found - run 02_Users_Seed.ps1 first" }
Write-Host ("Resolved {0} organizations, creator user_id={1}" -f $orgLookup.Count, $creatorId)

# --- Programmes data ---
$programmes = @(
    @{code='PROG-NUTRI-2026-01'; org='Sunrise Education Foundation';      title='Saturday Community Kitchen';          type='Nutrition';       desc='Weekly nutritious meal preparation and distribution to children in 3 care homes in District 5.';                                                              loc='District 5, Ho Chi Minh City';   daysAgo=20; daysFwd=60;  benef=240; budget=18000000; actual=15500000; status='Ongoing';   img='https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800'; feat=1; max=30},
    @{code='PROG-NUTRI-2026-02'; org='GreenLeaf Community Care';          title='Weekend Meal Drive';                  type='Nutrition';       desc='Monthly weekend meal kit distribution covering 12 districts in Ho Chi Minh City for underprivileged families.';                                              loc='12 districts, Ho Chi Minh City';  daysAgo=10; daysFwd=45;  benef=400; budget=32000000; actual=28000000; status='Ongoing';   img='https://images.unsplash.com/photo-1593113598332-cd288d649433?w=800'; feat=0; max=20},
    @{code='PROG-NUTRI-2026-03'; org='Pearl Health Alliance';             title='School Lunch Programme';              type='Nutrition';       desc='Daily school lunch for 350 students at 3 primary schools in Binh Chanh district.';                                                                                 loc='Binh Chanh, Ho Chi Minh City';   daysAgo=90; daysFwd=180; benef=350; budget=72000000; actual=68000000; status='Ongoing';    img='https://images.unsplash.com/photo-1504754524776-8f4f37790ca0?w=800'; feat=1; max=10},
    @{code='PROG-EDU-2026-01';   org='Sunrise Education Foundation';      title='Back-to-School Kit Distribution 2026'; type='Education';       desc='Backpacks, textbooks and full learning supplies for 500 children across 10 schools.';                                                                             loc='Multiple districts, HCMC';       daysAgo=5;  daysFwd=30;  benef=500; budget=95000000; actual=85000000; status='Ongoing';    img='https://images.unsplash.com/photo-1497486751825-1233686d5d80?w=800'; feat=1; max=50},
    @{code='PROG-EDU-2026-02';   org='Bright Path Children NGO';          title='Weekend Reading Club';                type='Education';       desc='Saturday morning reading circles with volunteer mentors for children ages 6-12.';                                                                                  loc='District 3 Library';             daysAgo=14; daysFwd=90;  benef=80;  budget=9000000;  actual=7200000;  status='Ongoing';   img='https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=800'; feat=0; max=15},
    @{code='PROG-EDU-2026-03';   org='Mekong Delta Youth Trust';          title='Digital Literacy Workshop';           type='Education';       desc='Two-week intensive computer literacy and online safety course for teens in rural Mekong provinces.';                                                              loc='Can Tho, Mekong Delta';          daysAgo=60; daysFwd=120; benef=60;  budget=42000000; actual=38000000; status='Ongoing';    img='https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800'; feat=0; max=20},
    @{code='PROG-EDU-2026-04';   org='Education For All';                 title='Scholarship Awarding Ceremony 2026';  type='Education';       desc='Annual ceremony awarding 30 university scholarships to outstanding students from low-income families.';                                                            loc='Reunification Palace';           daysAgo=120; daysFwd=0;   benef=30;  budget=450000000;actual=420000000;status='Completed'; img='https://images.unsplash.com/photo-1523050854058-8df90110c9f1?w=800'; feat=1; max=200},
    @{code='PROG-HEALTH-2026-01';org='Pearl Health Alliance';             title='Mobile Health Clinic - Quarter 2';    type='Healthcare';      desc='Mobile clinic visiting 8 remote communes providing free check-ups, medicines and vaccinations.';                                                                  loc='Tien Giang, Long An, Ben Tre';   daysAgo=45; daysFwd=15;  benef=600; budget=85000000; actual=72000000; status='Ongoing';   img='https://images.unsplash.com/photo-1576091160550-2173dba999ef?w=800'; feat=1; max=25},
    @{code='PROG-HEALTH-2026-02';org='Community Health Network';          title='Maternal Care Programme';             type='Healthcare';      desc='Pre/postnatal care packages and nutrition guidance for expectant mothers in care homes.';                                                                              loc='District 7, HCMC';               daysAgo=100;daysFwd=180; benef=150; budget=55000000; actual=48000000; status='Ongoing';    img='https://images.unsplash.com/photo-1559757175-5700dde675bc?w=800'; feat=0; max=15},
    @{code='PROG-HEALTH-2026-03';org='GreenLeaf Community Care';          title='Vaccination Awareness Day';           type='Healthcare';      desc='Community event bringing local doctors to explain childhood vaccination schedules to parents.';                                                                       loc='District 10 Community Hall';     daysAgo=2;  daysFwd=5;   benef=200; budget=8000000;  actual=7500000;  status='Upcoming';  img='https://images.unsplash.com/photo-1584516150909-c43483ee7932?w=800'; feat=0; max=100},
    @{code='PROG-SHELTER-2026-01';org='Hope Foundation';                  title='Warm Winter Clothing Drive';          type='Shelter';         desc='Distribution of jackets, blankets and warm clothing to children in mountain-region care homes.';                                                                   loc='Da Lat, Lam Dong';               daysAgo=130;daysFwd=-5;  benef=300; budget=60000000; actual=58000000; status='Completed'; img='https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?w=800'; feat=0; max=40},
    @{code='PROG-SHELTER-2026-02';org='Mekong Logistics Co., Ltd.';       title='Care Home Renovation Phase 2';        type='Shelter';         desc='Renovation of dormitories, kitchen facilities and play areas at 5 care homes in the Mekong Delta.';                                                                  loc='Can Tho, Soc Trang';             daysAgo=70; daysFwd=60;  benef=180; budget=180000000;actual=145000000;status='Ongoing';   img='https://images.unsplash.com/photo-1581094794329-c8112a89af12?w=800'; feat=1; max=20},
    @{code='PROG-COMMUNITY-2026-01';org='Children First Donor Circle';     title='Children Day Festival 2026';          type='Community Event'; desc='Annual celebration with games, performances and gifts for 1,000+ children across all partner care homes.';                                                            loc='Le Thanh Ton Park, HCMC';        daysAgo=100;daysFwd=200; benef=1000;budget=75000000; actual=62000000; status='Ongoing';    img='https://images.unsplash.com/photo-1530268729831-4b0b9e170218?w=800'; feat=1; max=80},
    @{code='PROG-COMMUNITY-2026-02';org='Bright Future Tech';              title='Volunteer Appreciation Gala';         type='Community Event'; desc='Annual gala dinner recognizing 200+ outstanding volunteers with awards and certificates.';                                                                            loc='Rex Hotel Ballroom';             daysAgo=160;daysFwd=-10; benef=200; budget=120000000;actual=115000000;status='Completed'; img='https://images.unsplash.com/photo-1511795409834-ef04bbd61622?w=800'; feat=0; max=250}
)

# --- Phase 2: insert ---
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $inserted = 0
    $skipped = 0
    $sql = 'INSERT INTO dbo.Programmes (organization_id, title, programme_type, description, start_date, end_date, location, target_beneficiaries, expected_budget, actual_budget, status, image_url, is_featured, registration_required, max_participants, created_at, updated_at, created_by) VALUES (@org, @title, @type, @desc, DATEADD(day, @daysAgo, CAST(GETDATE() AS DATE)), DATEADD(day, @daysFwd, CAST(GETDATE() AS DATE)), @loc, @benef, @budget, @actual, @status, @img, @feat, 1, @max, DATEADD(day, @daysAgo, GETDATE()), GETDATE(), @creator)'

    foreach ($p in $programmes) {
        if (-not $orgLookup.ContainsKey($p.org)) {
            Write-Host ("[WARN] Organization not found: {0} - skipping {1}" -f $p.org, $p.code)
            continue
        }
        $orgId = $orgLookup[$p.org]

        $checkCmd = $conn.CreateCommand()
        $checkCmd.CommandTimeout = 30
        $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.Programmes WHERE title = @t AND organization_id = @org'
        $checkCmd.Parameters.AddWithValue('@t', $p.title) | Out-Null
        $checkCmd.Parameters.AddWithValue('@org', $orgId) | Out-Null
        $exists = [int]$checkCmd.ExecuteScalar()
        if ($exists -gt 0) {
            Write-Host ("[SKIP] {0,-28} {1}" -f $p.code, $p.title)
            $skipped++
            continue
        }

        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 30
        $cmd.CommandText = $sql
        $cmd.Parameters.AddWithValue('@org', $orgId)          | Out-Null
        $cmd.Parameters.AddWithValue('@title', $p.title)      | Out-Null
        $cmd.Parameters.AddWithValue('@type', $p.type)        | Out-Null
        $cmd.Parameters.AddWithValue('@desc', $p.desc)        | Out-Null
        $cmd.Parameters.AddWithValue('@daysAgo', -$p.daysAgo) | Out-Null
        $cmd.Parameters.AddWithValue('@daysFwd', $p.daysFwd)  | Out-Null
        $cmd.Parameters.AddWithValue('@loc', $p.loc)          | Out-Null
        $cmd.Parameters.AddWithValue('@benef', $p.benef)      | Out-Null
        $cmd.Parameters.AddWithValue('@budget', $p.budget)    | Out-Null
        $cmd.Parameters.AddWithValue('@actual', $p.actual)    | Out-Null
        $cmd.Parameters.AddWithValue('@status', $p.status)    | Out-Null
        $cmd.Parameters.AddWithValue('@img', $p.img)          | Out-Null
        $cmd.Parameters.AddWithValue('@feat', $p.feat)        | Out-Null
        $cmd.Parameters.AddWithValue('@max', $p.max)          | Out-Null
        $cmd.Parameters.AddWithValue('@creator', $creatorId)  | Out-Null

        $rows = $cmd.ExecuteNonQuery()
        Write-Host ("[OK]   {0,-28} {1,-40} {2}" -f $p.code, $p.title, $p.status)
        $inserted++
        Start-Sleep -Milliseconds 250
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Programmes seed DONE. Inserted: {0}, Skipped: {1}" -f $inserted, $skipped)
    Write-Host '===================================================='
} finally {
    $conn.Close(); $conn.Dispose()
}
