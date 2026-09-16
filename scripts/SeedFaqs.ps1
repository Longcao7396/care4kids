param([string]$Server='.\SQLEXPRESS,62580', [string]$Database='GiveAIDDB')

# FAQ items - (id, question, answer, category, display_order, is_featured)
$faqs = @(
    @(1, 'How do I make a donation to Care4Kids?',
      'Making a donation to Care4Kids is quick and secure. Visit our Donate page, choose an amount that feels right for you, and complete the payment form. We accept credit cards, debit cards and local bank transfer. Every donation goes directly towards food, education, healthcare and safe shelter for vulnerable children across Vietnam.',
      'Donations', 1, 1),
    @(2, 'Is my donation to Care4Kids tax-deductible?',
      'Yes. Care4Kids is a registered non-profit organization in Vietnam. Official donation receipts are issued for every contribution above 100,000 VND and can be used to support your tax declaration according to local regulations.',
      'Donations', 2, 1),
    @(3, 'Can I donate anonymously?',
      'Yes. During the donation flow you can tick the "Donate anonymously" option and your name will not appear on public donor lists or in campaign reports. The transaction itself is still recorded for financial transparency.',
      'Donations', 3, 0),
    @(4, 'How is my donation used?',
      'Every donation to Care4Kids is allocated to the cause or campaign you choose. We publish a financial report for every active campaign showing exactly how funds were spent.',
      'Donations', 4, 1),
    @(5, 'How do I register for a Care4Kids programme?',
      'Visit the Programmes page, find the programme that interests you, and click Register. You will need a free Care4Kids account.',
      'Programmes', 5, 1),
    @(6, 'Can I cancel my programme registration?',
      'Yes. You can cancel any registration before the programme starts from your My Registrations page.',
      'Programmes', 6, 0),
    @(7, 'Are Care4Kids programmes free to attend?',
      'Most of our community programmes are free. Some specialised events may charge a small materials fee, clearly listed on the programme page.',
      'Programmes', 7, 0),
    @(8, 'How do I create a Care4Kids account?',
      'Click Register in the top navigation, fill in your name, email and a password.',
      'Account', 8, 1),
    @(9, 'I forgot my password. How do I reset it?',
      'On the Login page, click "Forgot password" and enter the email address on your account.',
      'Account', 9, 0),
    @(10, 'Is my personal information secure?',
      'Care4Kids uses industry-standard encryption (HTTPS, TLS 1.2+) for all data in transit and at rest.',
      'Account', 10, 1),
    @(11, 'Does Care4Kids accept volunteers?',
      'Yes! We welcome volunteers for tutoring, meal service, weekend activities, photography, translation and professional skills.',
      'Volunteering', 11, 1),
    @(12, 'How can my organization partner with Care4Kids?',
      'Send a brief introduction to partners@care4kids.example.org and our partnerships team will respond within 5 working days.',
      'Volunteering', 12, 0),
    @(13, 'Who are the children that Care4Kids supports?',
      'Care4Kids works with children living in care homes, children from low-income families, and children in remote provinces.',
      'Child Welfare', 13, 1),
    @(14, 'How can I contact the Care4Kids team?',
      'Reach us through our Contact form, by email at hello@care4kids.example.org, or by phone during business hours.',
      'General', 14, 1),
    @(15, 'How does Care4Kids protect children''s privacy and safety?',
      'Every photo and story is reviewed by our safeguarding officer. We never publish identifying details without written guardian consent.',
      'Child Welfare', 15, 0)
)

Add-Type -AssemblyName System.Data
$conn = New-Object System.Data.SqlClient.SqlConnection "Server=$Server;Database=$Database;Integrated Security=True;TrustServerCertificate=True;Connect Timeout=60;Pooling=False"
$conn.Open()
try {
    foreach ($f in $faqs) {
        $id = $f[0]; $q = $f[1]; $a = $f[2]; $cat = $f[3]; $ord = $f[4]; $feat = $f[5]
        # Escape single quotes in T-SQL by doubling them
        $qEsc = $q -replace "'", "''"
        $aEsc = $a -replace "'", "''"
        $catEsc = $cat -replace "'", "''"

        $sql = @"
IF EXISTS (SELECT 1 FROM dbo.Faqs WHERE faq_id = $id)
BEGIN
    UPDATE dbo.Faqs SET
        question = N'$qEsc',
        answer = N'$aEsc',
        category = N'$catEsc',
        display_order = $ord,
        is_active = 1,
        is_featured = $feat,
        updated_at = GETDATE()
    WHERE faq_id = $id;
END
ELSE
BEGIN
    SET IDENTITY_INSERT dbo.Faqs ON;
    INSERT INTO dbo.Faqs (faq_id, question, answer, category, display_order, is_active, is_featured, view_count, created_at, updated_at)
    VALUES ($id, N'$qEsc', N'$aEsc', N'$catEsc', $ord, 1, $feat, 0, GETDATE(), GETDATE());
    SET IDENTITY_INSERT dbo.Faqs OFF;
END
"@
        $cmd = $conn.CreateCommand()
        $cmd.CommandTimeout = 60
        $cmd.CommandText = $sql
        $rows = $cmd.ExecuteNonQuery()
        Write-Host "FAQ $id ($cat): rows=$rows"
        Start-Sleep -Milliseconds 300
    }
} finally {
    $conn.Close(); $conn.Dispose()
}
Write-Host "DONE: 15 FAQs processed."
