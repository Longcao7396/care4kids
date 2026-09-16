# 07_Gallery_Seed.ps1
# Seed 36 gallery items linked to programmes and organizations.
# Categories: Education, Meals, Healthcare, Shelter, Volunteers, Events, Children.
# Idempotent: checks (title, photo_url) before insert.

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
$orgLookup = @{}
$creatorId = 0
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT programme_id, title FROM dbo.Programmes'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) { $progLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = 'SELECT organization_id, organization_name FROM dbo.Organizations'
    $rdr = $cmd2.ExecuteReader()
    while ($rdr.Read()) { $orgLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd3 = $conn.CreateCommand()
    $cmd3.CommandText = "SELECT user_id FROM dbo.Users WHERE username = 'content.lead'"
    $creatorId = [int]$cmd3.ExecuteScalar()
} finally {
    $conn.Close(); $conn.Dispose()
}

if ($creatorId -eq 0) { throw "content.lead user not found" }

# Gallery: title | category | tags | programme_title | org_name | is_featured | photo_url
$gallery = @(
    @{title='Children receiving school kits'; category='Education'; tags='backpack,textbook,children,school'; prog='Back-to-School Kit Distribution 2026'; org='Sunrise Education Foundation'; feat=1; url='https://images.unsplash.com/photo-1497486751825-1233686d5d80?w=800'},
    @{title='Reading circle at District 3'; category='Education'; tags='reading,library,volunteers'; prog='Weekend Reading Club'; org='Bright Path Children NGO'; feat=0; url='https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=800'},
    @{title='Digital literacy class'; category='Education'; tags='computer,workshop,teens'; prog='Digital Literacy Workshop'; org='Mekong Delta Youth Trust'; feat=0; url='https://images.unsplash.com/photo-1517694712202-14dd9538aa97?w=800'},
    @{title='Scholarship recipients 2026'; category='Education'; tags='scholarship,ceremony,students'; prog='Scholarship Awarding Ceremony 2026'; org='Education For All'; feat=1; url='https://images.unsplash.com/photo-1523050854058-8df90110c9f1?w=800'},
    @{title='Group study session'; category='Education'; tags='study,tutoring'; prog='Weekend Reading Club'; org='Bright Path Children NGO'; feat=0; url='https://images.unsplash.com/photo-1577896851231-70ef18881754?w=800'},
    @{title='School supplies packing day'; category='Education'; tags='volunteers,packing'; prog='Back-to-School Kit Distribution 2026'; org='Children First Donor Circle'; feat=0; url='https://images.unsplash.com/photo-1593113598332-cd288d649433?w=800'},
    @{title='Online class attendance'; category='Education'; tags='online,remote-learning'; prog='Digital Literacy Workshop'; org='Mekong Delta Youth Trust'; feat=0; url='https://images.unsplash.com/photo-1588072432836-e10032774350?w=800'},
    @{title='Teacher training workshop'; category='Education'; tags='teachers,training'; prog='Weekend Reading Club'; org='Ho Chi Minh City Teachers'' Union'; feat=0; url='https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=800'},

    @{title='Hot meal distribution at District 5'; category='Meals'; tags='meals,nutrition,children'; prog='Saturday Community Kitchen'; org='Sunrise Education Foundation'; feat=1; url='https://images.unsplash.com/photo-1488521787991-ed7bbaae773c?w=800'},
    @{title='Meal kit packing line'; category='Meals'; tags='meal-kit,volunteers'; prog='Weekend Meal Drive'; org='GreenLeaf Community Care'; feat=0; url='https://images.unsplash.com/photo-1593113598332-cd288d649433?w=800'},
    @{title='School lunch service'; category='Meals'; tags='lunch,school'; prog='School Lunch Programme'; org='Pearl Health Alliance'; feat=0; url='https://images.unsplash.com/photo-1504754524776-8f4f37790ca0?w=800'},
    @{title='Community kitchen team'; category='Meals'; tags='kitchen,team,chef'; prog='Saturday Community Kitchen'; org='Sunrise Education Foundation'; feat=0; url='https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=800'},
    @{title='Children enjoying hot meal'; category='Meals'; tags='children,happy,meal'; prog='School Lunch Programme'; org='Pearl Health Alliance'; feat=1; url='https://images.unsplash.com/photo-1571115764595-644a1f56a55c?w=800'},

    @{title='Mobile clinic arrival in remote village'; category='Healthcare'; tags='mobile-clinic,rural'; prog='Mobile Health Clinic - Quarter 2'; org='Pearl Health Alliance'; feat=1; url='https://images.unsplash.com/photo-1576091160550-2173dba999ef?w=800'},
    @{title='Doctor examining children'; category='Healthcare'; tags='doctor,check-up'; prog='Mobile Health Clinic - Quarter 2'; org='Pearl Health Alliance'; feat=0; url='https://images.unsplash.com/photo-1559757175-5700dde675bc?w=800'},
    @{title='Vaccination day at community center'; category='Healthcare'; tags='vaccination,community'; prog='Vaccination Awareness Day'; org='GreenLeaf Community Care'; feat=0; url='https://images.unsplash.com/photo-1584516150909-c43483ee7932?w=800'},
    @{title='Maternal care consultation'; category='Healthcare'; tags='maternal,pregnancy'; prog='Maternal Care Programme'; org='Community Health Network'; feat=0; url='https://images.unsplash.com/photo-1551884170-09fb70a3a2ed?w=800'},
    @{title='Medical supplies distribution'; category='Healthcare'; tags='medicines,supplies'; prog='Mobile Health Clinic - Quarter 2'; org='Pearl Health Alliance'; feat=0; url='https://images.unsplash.com/photo-1606206873764-fd15e362df28?w=800'},

    @{title='Care home dormitory after renovation'; category='Shelter'; tags='renovation,dormitory'; prog='Care Home Renovation Phase 2'; org='Mekong Logistics Co., Ltd.'; feat=1; url='https://images.unsplash.com/photo-1581094794329-c8112a89af12?w=800'},
    @{title='New kitchen installation'; category='Shelter'; tags='kitchen,facility'; prog='Care Home Renovation Phase 2'; org='Mekong Logistics Co., Ltd.'; feat=0; url='https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=800'},
    @{title='Warm clothing kits packed'; category='Shelter'; tags='clothing,winter'; prog='Warm Winter Clothing Drive'; org='Hope Foundation'; feat=0; url='https://images.unsplash.com/photo-1469571486292-0ba58a3f068b?w=800'},
    @{title='Bedroom makeover completion'; category='Shelter'; tags='bedroom,children'; prog='Care Home Renovation Phase 2'; org='Mekong Logistics Co., Ltd.'; feat=0; url='https://images.unsplash.com/photo-1505693416388-ac5ce068fe85?w=800'},

    @{title='Volunteer team orientation'; category='Volunteers'; tags='volunteers,orientation'; prog='Volunteer Appreciation Gala'; org='Bright Future Tech'; feat=0; url='https://images.unsplash.com/photo-1559027615-cd4628902d4a?w=800'},
    @{title='Volunteers packing meal kits'; category='Volunteers'; tags='volunteers,meals'; prog='Saturday Community Kitchen'; org='Sunrise Education Foundation'; feat=1; url='https://images.unsplash.com/photo-1593113598332-cd288d649433?w=800'},
    @{title='Volunteer appreciation night'; category='Volunteers'; tags='gala,awards'; prog='Volunteer Appreciation Gala'; org='Bright Future Tech'; feat=1; url='https://images.unsplash.com/photo-1511795409834-ef04bbd61622?w=800'},
    @{title='Volunteer tutor with student'; category='Volunteers'; tags='tutoring,mentoring'; prog='Weekend Reading Club'; org='Bright Path Children NGO'; feat=0; url='https://images.unsplash.com/photo-1571260899304-425eee4c7efc?w=800'},
    @{title='Volunteer medical team'; category='Volunteers'; tags='volunteers,medical'; prog='Mobile Health Clinic - Quarter 2'; org='Pearl Health Alliance'; feat=0; url='https://images.unsplash.com/photo-1584467735815-f778f274e296?w=800'},

    @{title='Children Day Festival opening ceremony'; category='Events'; tags='festival,ceremony,children'; prog='Children Day Festival 2026'; org='Children First Donor Circle'; feat=1; url='https://images.unsplash.com/photo-1530268729831-4b0b9e170218?w=800'},
    @{title='Festival performance stage'; category='Events'; tags='performance,stage'; prog='Children Day Festival 2026'; org='Children First Donor Circle'; feat=0; url='https://images.unsplash.com/photo-1492684223066-81342ee5ff30?w=800'},
    @{title='Children receiving gifts'; category='Events'; tags='gifts,festival'; prog='Children Day Festival 2026'; org='Children First Donor Circle'; feat=0; url='https://images.unsplash.com/photo-1513151233558-d860c5398176?w=800'},
    @{title='Annual gala dinner'; category='Events'; tags='gala,dinner'; prog='Volunteer Appreciation Gala'; org='Bright Future Tech'; feat=0; url='https://images.unsplash.com/photo-1519741497674-611481863552?w=800'},
    @{title='Outdoor community gathering'; category='Events'; tags='community,gathering'; prog='Children Day Festival 2026'; org='Children First Donor Circle'; feat=0; url='https://images.unsplash.com/photo-1529156069898-49953e39b3ac?w=800'},

    @{title='Children playing at care home'; category='Children'; tags='children,play,happy'; prog='Care Home Renovation Phase 2'; org='Mekong Logistics Co., Ltd.'; feat=1; url='https://images.unsplash.com/photo-1602052793312-b779c08a90b6?w=800'},
    @{title='Children art class'; category='Children'; tags='art,children,creative'; prog='Weekend Reading Club'; org='Bright Path Children NGO'; feat=0; url='https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=800'},
    @{title='Group photo of care home kids'; category='Children'; tags='group-photo,kids'; prog='Care Home Renovation Phase 2'; org='Mekong Logistics Co., Ltd.'; feat=0; url='https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?w=800'},
    @{title='Smiling child holding backpack'; category='Children'; tags='child,backpack,happy'; prog='Back-to-School Kit Distribution 2026'; org='Sunrise Education Foundation'; feat=0; url='https://images.unsplash.com/photo-1503454537195-1dcabb73ffb9?w=800'}
)

$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $inserted = 0
    $skipped = 0
    $sql = 'INSERT INTO dbo.Gallery (title, photo_url, thumbnail_url, category, tags, programme_id, organization_id, display_order, is_featured, uploaded_by, uploaded_at) VALUES (@title, @url, @url, @cat, @tags, @pid, @oid, @ord, @feat, @uid, GETDATE())'

    $i = 0
    foreach ($g in $gallery) {
        $i++
        $progId = if ($g.prog -and $progLookup.ContainsKey($g.prog)) { $progLookup[$g.prog] } else { $null }
        $orgId = if ($g.org -and $orgLookup.ContainsKey($g.org)) { $orgLookup[$g.org] } else { $null }

        $checkCmd = $conn.CreateCommand()
        $checkCmd.CommandTimeout = 60
        $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.Gallery WHERE title = @t AND photo_url = @u'
        $checkCmd.Parameters.AddWithValue('@t', $g.title) | Out-Null
        $checkCmd.Parameters.AddWithValue('@u', $g.url)   | Out-Null
        if ([int]$checkCmd.ExecuteScalar() -gt 0) {
            Write-Host ("[SKIP] {0}" -f $g.title)
            $skipped++
            continue
        }

        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 60
        $cmd.CommandText = $sql
        $cmd.Parameters.AddWithValue('@title', $g.title)    | Out-Null
        $cmd.Parameters.AddWithValue('@url', $g.url)        | Out-Null
        $cmd.Parameters.AddWithValue('@cat', $g.category)   | Out-Null
        $cmd.Parameters.AddWithValue('@tags', $g.tags)      | Out-Null
        if ($progId) { $cmd.Parameters.AddWithValue('@pid', $progId) | Out-Null } else { $cmd.Parameters.AddWithValue('@pid', [DBNull]::Value) | Out-Null }
        if ($orgId)  { $cmd.Parameters.AddWithValue('@oid', $orgId) | Out-Null } else { $cmd.Parameters.AddWithValue('@oid', [DBNull]::Value) | Out-Null }
        $cmd.Parameters.AddWithValue('@ord', $i)            | Out-Null
        $cmd.Parameters.AddWithValue('@feat', $g.feat)      | Out-Null
        $cmd.Parameters.AddWithValue('@uid', $creatorId)    | Out-Null

        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host ("[OK] {0,-45} {1}" -f $g.title, $g.category)
        $inserted++
        Start-Sleep -Milliseconds 150
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Gallery seed DONE. Inserted: {0}, Skipped: {1}" -f $inserted, $skipped)
    Write-Host '===================================================='
} finally {
    $conn.Close(); $conn.Dispose()
}
