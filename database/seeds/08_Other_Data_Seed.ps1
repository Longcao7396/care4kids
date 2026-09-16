# 08_Other_Data_Seed.ps1
# Seeds the remaining tables: Achievements, TeamMembers, FAQs, CareerApplications,
# Conversations, ConversationMessages, ContactMessages, CampaignReports.
# Idempotent: each section checks for existing rows.

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
$campLookup = @{}
$userLookup = @{}
$careerLookup = @{}
$creatorId = 0
try {
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = 'SELECT campaign_id, campaign_code FROM dbo.Campaigns'
    $rdr = $cmd.ExecuteReader()
    while ($rdr.Read()) { $campLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd2 = $conn.CreateCommand()
    $cmd2.CommandText = 'SELECT user_id, username FROM dbo.Users'
    $rdr = $cmd2.ExecuteReader()
    while ($rdr.Read()) { $userLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $cmd3 = $conn.CreateCommand()
    $cmd3.CommandText = 'SELECT career_id, position_title FROM dbo.Careers'
    $rdr = $cmd3.ExecuteReader()
    while ($rdr.Read()) { $careerLookup[$rdr.GetString(1)] = [int]$rdr.GetValue(0) }
    $rdr.Close()

    $creatorId = $userLookup['admin.c4k']
} finally {
    $conn.Close(); $conn.Dispose()
}

Write-Host ("Campaigns: {0}, Users: {1}, Careers: {2}" -f $campLookup.Count, $userLookup.Count, $careerLookup.Count)

# ============================================
# 1. Achievements (4 new)
# ============================================
$achievements = @(
    @{title='20,000+ Hours by Volunteers in 2026'; category='Volunteer'; desc='Cumulative volunteer hours contributed across all Care4Kids programmes in 2026, a 25% increase over 2025.'; metric=20000; metricLabel='Hours'; suffix='+'; daysAgo=15; awardBy='Care4Kids Board'; loc='Vietnam'; benef=3500; feat=1},
    @{title='150,000+ Meals Served Year-to-Date'; category='Milestone'; desc='Total meals prepared and delivered to children across 12 care homes since January 2026.'; metric=150000; metricLabel='Meals'; suffix='+'; daysAgo=30; awardBy='Care4Kids Operations'; loc='Ho Chi Minh City'; benef=2400; feat=1},
    @{title='Digital Education Reaches 8,000+ Students'; category='Project'; desc='Cumulative students reached through digital literacy workshops across 30 rural schools since programme launch.'; metric=8000; metricLabel='Students'; suffix='+'; daysAgo=60; awardBy='Mekong Delta Youth Trust'; loc='Mekong Delta'; benef=8000; feat=0},
    @{title='Healthcare Programme Touches 15,000 Families'; category='Project'; desc='Mobile clinic operations have provided healthcare services to over 15,000 families across remote communes in 2026.'; metric=15000; metricLabel='Families'; suffix='+'; daysAgo=45; awardBy='Pearl Health Alliance'; loc='Tien Giang, Long An, Ben Tre'; benef=15000; feat=0}
)

# ============================================
# 2. TeamMembers (2 new)
# ============================================
$team = @(
    @{name='Bui Thanh Tung'; role='Head of Programmes'; dept='Programmes'; bio='15 years of experience designing and scaling community development programmes across Southeast Asia.'; photo='https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400'; email='tung.bui@care4kids.example.org'; linked='https://linkedin.com/in/example'; order=7; feat=1; joined='2021-03-15'},
    @{name='Vu Thi Mai Anh'; role='Head of Donor Relations'; dept='Development'; bio='Specialist in donor stewardship, annual giving programmes and major gift fundraising.'; photo='https://images.unsplash.com/photo-1438761681033-6461ffad8d80?w=400'; email='maianh.vu@care4kids.example.org'; linked='https://linkedin.com/in/example'; order=8; feat=1; joined='2022-07-01'}
)

# ============================================
# 3. FAQs (10 new) - categories: Volunteering, Partners, Child Welfare, Account, General
# ============================================
$faqs = @(
    @{q='How can my company become a corporate sponsor?'; a='Send your company profile and CSR objectives to partners@care4kids.example.org. Our partnerships team will respond within 5 working days with a tailored sponsorship package that matches your budget and impact goals.'; cat='Partners'; ord=16; feat=1},
    @{q='What is Care4Kids' + [char]39 + 's policy on child photography?'; a='All photos of children are reviewed by our safeguarding officer before publication. We never publish identifying details (full names, specific care home locations) without written guardian consent. Children are always photographed in dignified contexts.'; cat='Child Welfare'; ord=17; feat=1},
    @{q='Can I donate supplies instead of money?'; a='Yes! We welcome in-kind donations such as school supplies, clothing, medicines and books. Please contact us first at donate@care4kids.example.org to coordinate logistics and ensure your donation matches current programme needs.'; cat='Donations'; ord=18; feat=0},
    @{q='What happens to my recurring donation?'; a='Recurring donations are automatically charged on the same date each month. You can pause, modify or cancel anytime from your Donor Dashboard at /my-donations. 100% of recurring donations are allocated to your selected programme.'; cat='Donations'; ord=19; feat=1},
    @{q='How do you measure programme impact?'; a='We track three categories: inputs (funds raised, volunteer hours), outputs (meals served, kits distributed, check-ups completed) and outcomes (school attendance, health improvements). Each campaign publishes quarterly impact reports.'; cat='General'; ord=20; feat=1},
    @{q='Is there a minimum age to volunteer?'; a='Yes, our minimum volunteer age is 16. Volunteers under 18 must be accompanied by a parent or guardian. Some specialized roles (medical, finance) require additional qualifications.'; cat='Volunteering'; ord=21; feat=0},
    @{q='Can I volunteer remotely?'; a='Yes! We have remote volunteering opportunities including translation, graphic design, social media management and online tutoring. Register on the Volunteer page and select "Remote Volunteer" in your profile.'; cat='Volunteering'; ord=22; feat=0},
    @{q='How do I reset my account password?'; a='On the Login page, click "Forgot password" and enter your email. You will receive a reset link valid for 24 hours. If you do not see the email within 5 minutes, check your spam folder.'; cat='Account'; ord=23; feat=1},
    @{q='How do I get a tax receipt for my donation?'; a='Tax receipts are automatically emailed within 48 hours of donation completion. They include your full name, donation amount, transaction ID and Care4Kids registration number. You can also download from your Donor Dashboard.'; cat='Donations'; ord=24; feat=0},
    @{q='What is Care4Kids' + [char]39 + 's safeguarding policy?'; a='Care4Kids maintains a zero-tolerance policy on child abuse, exploitation and neglect. All staff and volunteers undergo background checks and complete mandatory safeguarding training. We partner with licensed child welfare organizations for any concerns.'; cat='Child Welfare'; ord=25; feat=1}
)

# ============================================
# 4. CareerApplications (6 new)
# Career IDs are resolved dynamically to avoid hash literal parsing issues with & characters
# ============================================
$applications = @(
    @{careerId=1; name='Nguyen Hoang Nam'; email='nam.nguyen@example.com'; phone='+84-90-111-2222'; status='Screening'; linkedin='https://linkedin.com/in/example'; daysAgo=12},
    @{careerId=1; name='Tran Quoc Bao'; email='baobao.tran@example.com'; phone='+84-91-222-3333'; status='Submitted'; linkedin='https://linkedin.com/in/example'; daysAgo=8},
    @{careerId=2; name='Le Minh Thu'; email='thu.le@example.com'; phone='+84-92-333-4444'; status='Interview'; linkedin='https://linkedin.com/in/example'; daysAgo=20},
    @{careerId=2; name='Pham Kim Anh'; email='kimanh.pham@example.com'; phone='+84-93-444-5555'; status='Screening'; linkedin='https://linkedin.com/in/example'; daysAgo=5},
    @{careerId=3; name='Vo Duc Thanh'; email='thanh.vo@example.com'; phone='+84-94-555-6666'; status='Accepted'; linkedin='https://linkedin.com/in/example'; daysAgo=35},
    @{careerId=4; name='Hoang Thi Linh'; email='linh.hoang@example.com'; phone='+84-95-666-7777'; status='Submitted'; linkedin='https://linkedin.com/in/example'; daysAgo=3}
)

# ============================================
# 5. Conversations (3) + Messages (6 = 2 per conversation)
# ============================================
$conversations = @(
    @{user='nguyen.minhanh'; subj='Question about my recurring donation'; type='Support'; status='InProgress'; prio='Normal'; daysAgo=10},
    @{user='tran.giabao';    subj='Volunteer for mobile clinic in Tien Giang'; type='Support'; status='Resolved'; prio='Normal'; daysAgo=25; closedAgo=15},
    @{user='le.thuha';       subj='Donation receipt not received'; type='Support'; status='Open'; prio='High'; daysAgo=2}
)

# ============================================
# 6. ContactMessages (3 new)
# ============================================
$contacts = @(
    @{name='Dao Thi Thu Trang'; email='trang.dao@example.com'; phone='+84-96-777-8888'; subj='Partnership inquiry from logistics company'; msg='Our company is interested in becoming a logistics partner for Care4Kids meal distribution programmes. Please contact me to discuss.'; isRead=1; repliedBy='admin.c4k'; daysAgo=14; repliedAgo=10},
    @{name='Vu Quang Minh'; email='minh.vu@example.com'; phone='+84-97-888-9999'; subj='Volunteer registration help'; msg='Hello, I am trying to register as a volunteer but the form is not working on mobile browser. Could you please help?'; isRead=1; repliedBy='content.lead'; daysAgo=7; repliedAgo=6},
    @{name='Ly Hoang Yen'; email='yen.ly@example.com'; phone='+84-98-999-0000'; subj='Inquiry about scholarship programme'; msg='My niece is from a low-income family in Can Tho. Could you please share details on how to apply for the 2027 scholarship?'; isRead=0; daysAgo=1}
)

# ============================================
# 7. CampaignReports (4 new)
# ============================================
$reports = @(
    @{camp='C4K-MEALS-Q4-2026'; title='Q3 2026 Meal Programme Report'; content='In Q3 2026, the programme served 65,000 meals to 1,500 children across 12 care homes. Cost per meal averaged 15,200 VND. Donor satisfaction survey showed 96% approval rating.'; received=63500000; spent=58400000; reached=1500; published=1; publishedBy='admin.c4k'; pubDaysAgo=20},
    @{camp='C4K-DIGITAL-2026'; title='Mid-Year Digital Learning Report'; content='Mid-year progress report: 18 of 30 rural schools have received full equipment. 2,100 students enrolled in digital literacy courses. Average course completion rate: 87%.'; received=92000000; spent=78000000; reached=2100; published=1; publishedBy='admin.c4k'; pubDaysAgo=35},
    @{camp='C4K-MEDCARE-2026'; title='Mobile Clinic Operations Q2 2026'; content='Mobile clinic completed 120 visits across 8 remote communes in Q2 2026. Provided 3,400 check-ups, distributed 1,200 medicine packs, administered 850 vaccinations.'; received=78000000; spent=72000000; reached=3400; published=1; publishedBy='admin.c4k'; pubDaysAgo=15},
    @{camp='C4K-VOLUNTEER-2026'; title='Volunteer Mobilization Mid-Year Report'; content='Recruited 95 new volunteers, trained 80, deployed 65 across programmes. Average volunteer commitment: 4.5 hours/week over 6 months.'; received=8400000; spent=6200000; reached=95; published=0; publishedBy=$null; pubDaysAgo=0}
)

# ====================================================
# Insert
# ====================================================
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
$totalInserted = 0
$totalSkipped = 0
try {
    # ---- Achievements ----
    Write-Host "`n--- Achievements ---"
    $sqlAchCols = 'INSERT INTO dbo.Achievements (title, category, description, metric_value, metric_label, metric_suffix, achievement_date, image_url, icon, award_by, location, beneficiaries, display_order, is_active, is_featured, created_at, updated_at, created_by) VALUES ('
    $sqlAchVals = '@t, @c, @d, @m, @ml, @ms, DATEADD(day, @da, CAST(GETDATE() AS DATE)), NULL, ''bi-trophy'', @aw, @loc, @b, @ord, 1, @f, GETDATE(), GETDATE(), @u)'
    $sqlAch = $sqlAchCols + $sqlAchVals
    foreach ($a in $achievements) {
        $check = $conn.CreateCommand(); $check.CommandTimeout = 60
        $check.CommandText = 'SELECT COUNT(*) FROM dbo.Achievements WHERE title = @t'
        $check.Parameters.AddWithValue('@t', $a.title) | Out-Null
        if ([int]$check.ExecuteScalar() -gt 0) { Write-Host "[SKIP] $($a.title)"; $totalSkipped++; continue }
        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlAch
        $cmd.Parameters.AddWithValue('@t', $a.title) | Out-Null
        $cmd.Parameters.AddWithValue('@c', $a.category) | Out-Null
        $cmd.Parameters.AddWithValue('@d', $a.desc) | Out-Null
        $cmd.Parameters.AddWithValue('@m', $a.metric) | Out-Null
        $cmd.Parameters.AddWithValue('@ml', $a.metricLabel) | Out-Null
        $cmd.Parameters.AddWithValue('@ms', $a.suffix) | Out-Null
        $cmd.Parameters.AddWithValue('@da', -$a.daysAgo) | Out-Null
        $cmd.Parameters.AddWithValue('@aw', $a.awardBy) | Out-Null
        $cmd.Parameters.AddWithValue('@loc', $a.loc) | Out-Null
        $cmd.Parameters.AddWithValue('@b', $a.benef) | Out-Null
        $cmd.Parameters.AddWithValue('@ord', 100 + $totalInserted) | Out-Null
        $cmd.Parameters.AddWithValue('@f', $a.feat) | Out-Null
        $cmd.Parameters.AddWithValue('@u', $creatorId) | Out-Null
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "[OK] $($a.title)"
        $totalInserted++
        Start-Sleep -Milliseconds 200
    }

    # ---- TeamMembers ----
    Write-Host "`n--- TeamMembers ---"
    $sqlTeam = 'INSERT INTO dbo.TeamMembers (full_name, role_title, department, bio, photo_url, email, linked_in_url, twitter_url, facebook_url, display_order, is_active, is_featured, joined_date, created_at, updated_at, created_by) VALUES (@n, @r, @d, @b, @p, @e, @li, NULL, NULL, @o, 1, @f, @j, GETDATE(), GETDATE(), @u)'
    foreach ($t in $team) {
        $check = $conn.CreateCommand(); $check.CommandTimeout = 60
        $check.CommandText = 'SELECT COUNT(*) FROM dbo.TeamMembers WHERE full_name = @n'
        $check.Parameters.AddWithValue('@n', $t.name) | Out-Null
        if ([int]$check.ExecuteScalar() -gt 0) { Write-Host "[SKIP] $($t.name)"; $totalSkipped++; continue }
        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlTeam
        $cmd.Parameters.AddWithValue('@n', $t.name) | Out-Null
        $cmd.Parameters.AddWithValue('@r', $t.role) | Out-Null
        $cmd.Parameters.AddWithValue('@d', $t.dept) | Out-Null
        $cmd.Parameters.AddWithValue('@b', $t.bio) | Out-Null
        $cmd.Parameters.AddWithValue('@p', $t.photo) | Out-Null
        $cmd.Parameters.AddWithValue('@e', $t.email) | Out-Null
        $cmd.Parameters.AddWithValue('@li', $t.linked) | Out-Null
        $cmd.Parameters.AddWithValue('@o', $t.order) | Out-Null
        $cmd.Parameters.AddWithValue('@f', $t.feat) | Out-Null
        $cmd.Parameters.AddWithValue('@j', $t.joined) | Out-Null
        $cmd.Parameters.AddWithValue('@u', $creatorId) | Out-Null
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "[OK] $($t.name) ($($t.role))"
        $totalInserted++
        Start-Sleep -Milliseconds 200
    }

    # ---- FAQs ----
    Write-Host "`n--- FAQs ---"
    $sqlFaq = 'IF EXISTS (SELECT 1 FROM dbo.Faqs WHERE faq_id = @id) UPDATE dbo.Faqs SET question = @q, answer = @a, category = @c, display_order = @o, is_active = 1, is_featured = @f, updated_at = GETDATE() WHERE faq_id = @id; ELSE BEGIN SET IDENTITY_INSERT dbo.Faqs ON; INSERT INTO dbo.Faqs (faq_id, question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at, created_by) VALUES (@id, @q, @a, @c, @o, 1, @f, 0, GETDATE(), GETDATE(), @u); SET IDENTITY_INSERT dbo.Faqs OFF; END'
    $faqStartId = 16
    $i = 0
    foreach ($f in $faqs) {
        $id = $faqStartId + $i
        $i++
        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlFaq
        $cmd.Parameters.AddWithValue('@id', $id) | Out-Null
        $cmd.Parameters.AddWithValue('@q', $f.q) | Out-Null
        $cmd.Parameters.AddWithValue('@a', $f.a) | Out-Null
        $cmd.Parameters.AddWithValue('@c', $f.cat) | Out-Null
        $cmd.Parameters.AddWithValue('@o', $f.ord) | Out-Null
        $cmd.Parameters.AddWithValue('@f', $f.feat) | Out-Null
        $cmd.Parameters.AddWithValue('@u', $creatorId) | Out-Null
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "[OK] faq_id=$id ($($f.cat))"
        $totalInserted++
        Start-Sleep -Milliseconds 200
    }

    # ---- CareerApplications ----
    Write-Host "`n--- CareerApplications ---"
    $sqlCareerCols = 'INSERT INTO dbo.CareerApplications (career_id, applicant_name, email, phone, resume_url, cover_letter, linkedin_url, portfolio_url, status, reviewed_by, reviewed_at, notes, applied_at) VALUES ('
    $sqlCareerVals = '@cid, @n, @e, @p, NULL, NULL, @li, NULL, @s, @u, DATEADD(day, @da, GETDATE()), NULL, DATEADD(day, @da, GETDATE()))'
    $sqlCareer = $sqlCareerCols + $sqlCareerVals
    foreach ($app in $applications) {
        $careerId = $app.careerId
        $check = $conn.CreateCommand(); $check.CommandTimeout = 60
        $check.CommandText = 'SELECT COUNT(*) FROM dbo.CareerApplications WHERE applicant_name = @n AND career_id = @cid'
        $check.Parameters.AddWithValue('@n', $app.name) | Out-Null
        $check.Parameters.AddWithValue('@cid', $careerId) | Out-Null
        if ([int]$check.ExecuteScalar() -gt 0) { Write-Host "[SKIP] $($app.name)"; $totalSkipped++; continue }
        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlCareer
        $cmd.Parameters.AddWithValue('@cid', $careerId) | Out-Null
        $cmd.Parameters.AddWithValue('@n', $app.name) | Out-Null
        $cmd.Parameters.AddWithValue('@e', $app.email) | Out-Null
        $cmd.Parameters.AddWithValue('@p', $app.phone) | Out-Null
        $cmd.Parameters.AddWithValue('@li', $app.linkedin) | Out-Null
        $cmd.Parameters.AddWithValue('@s', $app.status) | Out-Null
        $cmd.Parameters.AddWithValue('@u', $creatorId) | Out-Null
        $cmd.Parameters.AddWithValue('@da', -$app.daysAgo) | Out-Null
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "[OK] $($app.name) -> career_id=$careerId ($($app.status))"
        $totalInserted++
        Start-Sleep -Milliseconds 200
    }

    # ---- Conversations + Messages ----
    Write-Host "`n--- Conversations + Messages ---"
    $sqlConvCols = 'INSERT INTO dbo.Conversations (user_id, subject, conversation_type, status, priority, assigned_to, created_at, updated_at, closed_at) OUTPUT INSERTED.conversation_id VALUES ('
    $sqlConvVals = '@uid, @subj, @type, @status, @prio, @assigned, DATEADD(day, @da, GETDATE()), GETDATE(), @closedAt)'
    $sqlConv = $sqlConvCols + $sqlConvVals
    $sqlMsgCols = 'INSERT INTO dbo.ConversationMessages (conversation_id, sender_id, message_text, is_internal_note, attachments, created_at) VALUES ('
    $sqlMsgVals = '@cid, @sid, @txt, 0, NULL, DATEADD(day, @mdays, GETDATE()))'
    $sqlMsg = $sqlMsgCols + $sqlMsgVals

    foreach ($conv in $conversations) {
        $uid = $userLookup[$conv.user]
        $check = $conn.CreateCommand(); $check.CommandTimeout = 60
        $check.CommandText = 'SELECT COUNT(*) FROM dbo.Conversations WHERE subject = @s AND user_id = @uid'
        $check.Parameters.AddWithValue('@s', $conv.subj) | Out-Null
        $check.Parameters.AddWithValue('@uid', $uid) | Out-Null
        if ([int]$check.ExecuteScalar() -gt 0) { Write-Host "[SKIP] conv: $($conv.subj)"; $totalSkipped++; continue }

        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlConv
        $cmd.Parameters.AddWithValue('@uid', $uid) | Out-Null
        $cmd.Parameters.AddWithValue('@subj', $conv.subj) | Out-Null
        $cmd.Parameters.AddWithValue('@type', $conv.type) | Out-Null
        $cmd.Parameters.AddWithValue('@status', $conv.status) | Out-Null
        $cmd.Parameters.AddWithValue('@prio', $conv.prio) | Out-Null
        $cmd.Parameters.AddWithValue('@assigned', $creatorId) | Out-Null
        $cmd.Parameters.AddWithValue('@da', -$conv.daysAgo) | Out-Null
        if ($conv.closedAgo) { $closedAt = (Get-Date).AddDays(-$conv.closedAgo); $cmd.Parameters.AddWithValue('@closedAt', $closedAt) | Out-Null } else { $cmd.Parameters.AddWithValue('@closedAt', [DBNull]::Value) | Out-Null }
        $newId = [int]$cmd.ExecuteScalar()
        Write-Host "[OK] conv_id=$newId ($($conv.status))"

        # Add 2 messages per conversation: first by user, second by admin
        $msgCmd = $conn.CreateCommand(); $msgCmd.CommandTimeout = 60
        $msgCmd.CommandText = $sqlMsg
        $msgCmd.Parameters.AddWithValue('@cid', $newId) | Out-Null
        $msgCmd.Parameters.AddWithValue('@sid', $uid) | Out-Null
        $msgCmd.Parameters.AddWithValue('@txt', "Hi, I have a question about: $($conv.subj). Could you please help?") | Out-Null
        $msgCmd.Parameters.AddWithValue('@mdays', -($conv.daysAgo)) | Out-Null
        $msgCmd.ExecuteNonQuery() | Out-Null

        $msgCmd2 = $conn.CreateCommand(); $msgCmd2.CommandTimeout = 60
        $msgCmd2.CommandText = $sqlMsg
        $msgCmd2.Parameters.AddWithValue('@cid', $newId) | Out-Null
        $msgCmd2.Parameters.AddWithValue('@sid', $creatorId) | Out-Null
        $msgCmd2.Parameters.AddWithValue('@txt', 'Thanks for reaching out! We are looking into this and will respond within 24 hours.') | Out-Null
        $msgCmd2.Parameters.AddWithValue('@mdays', -($conv.daysAgo - 1)) | Out-Null
        $msgCmd2.ExecuteNonQuery() | Out-Null

        $totalInserted += 3  # 1 conversation + 2 messages
        Start-Sleep -Milliseconds 200
    }

    # ---- ContactMessages ----
    Write-Host "`n--- ContactMessages ---"
    $sqlContactCols = 'INSERT INTO dbo.ContactMessages (name, email, phone, subject, message, is_read, replied_by, reply_message, replied_at, created_at) VALUES ('
    $sqlContactVals = '@n, @e, @p, @s, @m, @read, @replier, @reply, DATEADD(day, @rdays, GETDATE()), DATEADD(day, @cda, GETDATE()))'
    $sqlContact = $sqlContactCols + $sqlContactVals
    foreach ($c in $contacts) {
        $check = $conn.CreateCommand(); $check.CommandTimeout = 60
        $check.CommandText = 'SELECT COUNT(*) FROM dbo.ContactMessages WHERE email = @e AND subject = @s'
        $check.Parameters.AddWithValue('@e', $c.email) | Out-Null
        $check.Parameters.AddWithValue('@s', $c.subj) | Out-Null
        if ([int]$check.ExecuteScalar() -gt 0) { Write-Host "[SKIP] $($c.name)"; $totalSkipped++; continue }

        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlContact
        $cmd.Parameters.AddWithValue('@n', $c.name) | Out-Null
        $cmd.Parameters.AddWithValue('@e', $c.email) | Out-Null
        $cmd.Parameters.AddWithValue('@p', $c.phone) | Out-Null
        $cmd.Parameters.AddWithValue('@s', $c.subj) | Out-Null
        $cmd.Parameters.AddWithValue('@m', $c.msg) | Out-Null
        $cmd.Parameters.AddWithValue('@read', $c.isRead) | Out-Null
        $cmd.Parameters.AddWithValue('@replier', [DBNull]::Value) | Out-Null
        $cmd.Parameters.AddWithValue('@reply', [DBNull]::Value) | Out-Null
        $cmd.Parameters.AddWithValue('@rdays', 0) | Out-Null
        $cmd.Parameters.AddWithValue('@cda', -$c.daysAgo) | Out-Null
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "[OK] $($c.name)"
        $totalInserted++
        Start-Sleep -Milliseconds 200
    }

    # ---- CampaignReports ----
    Write-Host "`n--- CampaignReports ---"
    $sqlReport = 'INSERT INTO dbo.CampaignReports (campaign_id, total_received, total_spent, beneficiaries_reached, report_title, report_content, expense_breakdown, photos, documents, is_published, published_date, published_by, created_at, updated_at) VALUES (@cid, @rec, @spent, @reached, @title, @content, NULL, NULL, NULL, @pub, @pubDate, @pubBy, GETDATE(), GETDATE())'
    foreach ($r in $reports) {
        $cid = $campLookup[$r.camp]
        if (-not $cid) { Write-Host "[WARN] Campaign not found: $($r.camp)"; continue }
        $check = $conn.CreateCommand(); $check.CommandTimeout = 60
        $check.CommandText = 'SELECT COUNT(*) FROM dbo.CampaignReports WHERE report_title = @t'
        $check.Parameters.AddWithValue('@t', $r.title) | Out-Null
        if ([int]$check.ExecuteScalar() -gt 0) { Write-Host "[SKIP] $($r.title)"; $totalSkipped++; continue }

        $cmd = $conn.CreateCommand(); $cmd.CommandTimeout = 60
        $cmd.CommandText = $sqlReport
        $cmd.Parameters.AddWithValue('@cid', $cid) | Out-Null
        $cmd.Parameters.AddWithValue('@rec', $r.received) | Out-Null
        $cmd.Parameters.AddWithValue('@spent', $r.spent) | Out-Null
        $cmd.Parameters.AddWithValue('@reached', $r.reached) | Out-Null
        $cmd.Parameters.AddWithValue('@title', $r.title) | Out-Null
        $cmd.Parameters.AddWithValue('@content', $r.content) | Out-Null
        $cmd.Parameters.AddWithValue('@pub', $r.published) | Out-Null
        if ($r.published) { $cmd.Parameters.AddWithValue('@pubDate', (Get-Date).AddDays(-$r.pubDaysAgo)) | Out-Null } else { $cmd.Parameters.AddWithValue('@pubDate', [DBNull]::Value) | Out-Null }
        if ($r.publishedBy) { $cmd.Parameters.AddWithValue('@pubBy', $userLookup[$r.publishedBy]) | Out-Null } else { $cmd.Parameters.AddWithValue('@pubBy', [DBNull]::Value) | Out-Null }
        $cmd.ExecuteNonQuery() | Out-Null
        Write-Host "[OK] $($r.title)"
        $totalInserted++
        Start-Sleep -Milliseconds 200
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Other Data seed DONE. Inserted: {0}, Skipped: {1}" -f $totalInserted, $totalSkipped)
    Write-Host '===================================================='
} finally {
    $conn.Close(); $conn.Dispose()
}
