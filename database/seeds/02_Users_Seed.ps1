# 02_Users_Seed.ps1
# Seed demo users: 1 Admin, 2 ContentManager, 6 Users (donors)
# Idempotent: checks by username/email before insert.
# All demo passwords = "Demo@1234" (BCrypt hash pre-computed).

param(
    [string]$Server = '.\SQLEXPRESS,62580',
    [string]$Database = 'GiveAIDDB'
)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Data

# BCrypt hash for "Demo@1234" generated with BCrypt.Net-Next 4.0.3 (cost 11)
$bcryptHash = '$2a$11$q9AS7WkIIocKkxu3luNSWu93ytEhyFLTMbkLDXHWr6KfcN4FvMGSW'

# Users: username|email|fullname|phone|address|profession|dob|gender|role|permissions|is_verified
$users = @(
    @{u='admin.c4k';      e='admin.c4k@care4kids.example.org';          n='Care4Kids Administrator'; p='+84-28-3550-0001'; a='5 Le Loi, District 1, Ho Chi Minh City';          pr='Charity Management';  d='1985-03-15'; g='Female'; r='SuperAdmin';     perm='["users.manage","campaigns.manage","donations.manage","reports.view","settings.manage"]'; ver=1},
    @{u='content.lead';   e='content.lead@care4kids.example.org';       n='Nguyen Thi Lan';          p='+84-28-3550-0002'; a='12 Nguyen Hue, District 1, Ho Chi Minh City';       pr='Content Strategy';    d='1990-07-22'; g='Female'; r='ContentManager'; perm='["campaigns.manage","faqs.manage","gallery.manage"]';                                                   ver=1},
    @{u='content.editor'; e='content.editor@care4kids.example.org';     n='Tran Van Minh';           p='+84-28-3550-0003'; a='78 Tran Hung Dao, District 5, Ho Chi Minh City';   pr='Communications';      d='1992-11-08'; g='Male';   r='ContentManager'; perm='["faqs.manage","gallery.manage"]';                                                                          ver=1},
    @{u='nguyen.minhanh'; e='nguyen.minhanh@example.com';               n='Nguyen Minh Anh';         p='+84-90-123-4567'; a='22 Bui Vien, District 1, Ho Chi Minh City';        pr='Software Engineer';   d='1995-04-12'; g='Female'; r='User';          perm=$null;                                                                                                       ver=1},
    @{u='tran.giabao';    e='tran.giabao@example.com';                  n='Tran Gia Bao';            p='+84-91-234-5678'; a='15 Pham Ngu Lao, District 1, Ho Chi Minh City';    pr='Marketing Manager';   d='1988-09-25'; g='Male';   r='User';          perm=$null;                                                                                                       ver=1},
    @{u='le.thuha';       e='le.thuha@example.com';                     n='Le Thu Ha';               p='+84-92-345-6789'; a='80 Cach Mang Thang 8, District 3, Ho Chi Minh City'; pr='Teacher';           d='1991-02-18'; g='Female'; r='User';          perm=$null;                                                                                                       ver=1},
    @{u='pham.ducminh';   e='pham.ducminh@example.com';                 n='Pham Duc Minh';           p='+84-93-456-7890'; a='33 Vo Van Tan, District 3, Ho Chi Minh City';      pr='Doctor';             d='1986-12-03'; g='Male';   r='User';          perm=$null;                                                                                                       ver=1},
    @{u='hoang.thanhthao';e='hoang.thanhthao@example.com';              n='Hoang Thanh Thao';        p='+84-94-567-8901'; a='90 Nguyen Thi Minh Khai, District 3, Ho Chi Minh City'; pr='Architect';       d='1993-06-30'; g='Female'; r='User';          perm=$null;                                                                                                       ver=1},
    @{u='do.quanghuy';    e='do.quanghuy@example.com';                  n='Do Quang Huy';            p='+84-95-678-9012'; a='55 Ly Tu Trong, District 1, Ho Chi Minh City';     pr='Financial Analyst';  d='1989-08-14'; g='Male';   r='User';          perm=$null;                                                                                                       ver=1}
)

$connStr = "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=60;Pooling=False"
$conn = New-Object System.Data.SqlClient.SqlConnection $connStr
$conn.Open()
try {
    $inserted = 0
    $skipped = 0

    $sql = @'
INSERT INTO dbo.Users
    (username, email, password_hash, full_name, phone, address, profession, date_of_birth, gender,
     role, permissions, is_verified, is_active, created_at, updated_at, last_login)
VALUES
    (@u, @e, @h, @n, @p, @a, @pr, @d, @g, @r, @perm, @ver, 1, GETDATE(), GETDATE(),
     DATEADD(day, @days, GETDATE()))
'@

    foreach ($u in $users) {
        $checkCmd = $conn.CreateCommand()
        $checkCmd.CommandTimeout = 30
        $checkCmd.CommandText = 'SELECT COUNT(*) FROM dbo.Users WHERE username = @u OR email = @e'
        $checkCmd.Parameters.AddWithValue('@u', $u.u) | Out-Null
        $checkCmd.Parameters.AddWithValue('@e', $u.e) | Out-Null
        $exists = [int]$checkCmd.ExecuteScalar()
        if ($exists -gt 0) {
            Write-Host ("[SKIP] {0,-22} {1}" -f $u.u, $u.e)
            $skipped++
            continue
        }

        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 30
        $cmd.CommandText = $sql
        $cmd.Parameters.AddWithValue('@u', $u.u)         | Out-Null
        $cmd.Parameters.AddWithValue('@e', $u.e)         | Out-Null
        $cmd.Parameters.AddWithValue('@h', $bcryptHash)  | Out-Null
        $cmd.Parameters.AddWithValue('@n', $u.n)         | Out-Null
        $cmd.Parameters.AddWithValue('@p', $u.p)         | Out-Null
        $cmd.Parameters.AddWithValue('@a', $u.a)         | Out-Null
        $cmd.Parameters.AddWithValue('@pr', $u.pr)       | Out-Null
        $cmd.Parameters.AddWithValue('@d', $u.d)         | Out-Null
        $cmd.Parameters.AddWithValue('@g', $u.g)         | Out-Null
        $cmd.Parameters.AddWithValue('@r', $u.r)         | Out-Null
        if ($u.perm) { $cmd.Parameters.AddWithValue('@perm', $u.perm) | Out-Null } else { $cmd.Parameters.AddWithValue('@perm', [DBNull]::Value) | Out-Null }
        $cmd.Parameters.AddWithValue('@ver', $u.ver)     | Out-Null
        $cmd.Parameters.AddWithValue('@days', -((Get-Random -Minimum 1 -Maximum 30))) | Out-Null

        $rows = $cmd.ExecuteNonQuery()
        Write-Host ("[OK]   {0,-22} {1,-45} {2}" -f $u.u, $u.e, $u.r)
        $inserted++
        Start-Sleep -Milliseconds 250
    }

    Write-Host ''
    Write-Host '===================================================='
    Write-Host ("Users seed DONE. Inserted: {0}, Skipped: {1}" -f $inserted, $skipped)
    Write-Host '===================================================='
} finally {
    $conn.Close()
    $conn.Dispose()
}
