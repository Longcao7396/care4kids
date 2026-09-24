namespace GiveAID.Infrastructure.Email;

public static class EmailTemplates
{
    public static string GetWelcomeEmailTemplate(string userName, string loginUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Welcome to GiveAID</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #2e7d32; color: white; padding: 20px; text-align: center;'>
        <h1>Welcome to GiveAID</h1>
    </div>
    <div style='padding: 20px; background-color: #f5f5f5;'>
        <h2>Hello {userName},</h2>
        <p>Thank you for joining GiveAID! Your journey to making a difference starts here.</p>
        <p>With GiveAID, you can:</p>
        <ul>
            <li>Discover and support meaningful causes</li>
            <li>Track your donations and impact</li>
            <li>Join volunteer programs in your community</li>
            <li>Connect with other changemakers</li>
        </ul>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{loginUrl}' style='background-color: #2e7d32; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px;'>Get Started</a>
        </p>
        <p>If you have any questions, please don't hesitate to contact us.</p>
        <p>Warm regards,<br>The GiveAID Team</p>
    </div>
</body>
</html>";
    }

    public static string GetDonationReceiptTemplate(string donorName, decimal amount, string currency, string causeName, string transactionId, DateTime donationDate)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Donation Receipt</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #1565c0; color: white; padding: 20px; text-align: center;'>
        <h1>Thank You for Your Donation!</h1>
    </div>
    <div style='padding: 20px; background-color: #f5f5f5;'>
        <p>Dear {donorName},</p>
        <p>Thank you for your generous donation! Your support makes a real difference.</p>
        <div style='background-color: white; padding: 20px; border-radius: 5px; margin: 20px 0;'>
            <h3 style='margin-top: 0;'>Donation Receipt</h3>
            <table style='width: 100%; border-collapse: collapse;'>
                <tr>
                    <td style='padding: 8px 0; border-bottom: 1px solid #eee;'><strong>Transaction ID:</strong></td>
                    <td style='padding: 8px 0; border-bottom: 1px solid #eee;'>{transactionId}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; border-bottom: 1px solid #eee;'><strong>Amount:</strong></td>
                    <td style='padding: 8px 0; border-bottom: 1px solid #eee;'>{amount:N0} {currency}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0; border-bottom: 1px solid #eee;'><strong>Cause:</strong></td>
                    <td style='padding: 8px 0; border-bottom: 1px solid #eee;'>{causeName}</td>
                </tr>
                <tr>
                    <td style='padding: 8px 0;'><strong>Date:</strong></td>
                    <td style='padding: 8px 0;'>{donationDate:MMMM dd, yyyy}</td>
                </tr>
            </table>
        </div>
        <p>Please keep this receipt for your records. Your donation is tax-deductible (if applicable).</p>
        <p>Warm regards,<br>The GiveAID Team</p>
    </div>
</body>
</html>";
    }

    public static string GetPasswordResetTemplate(string userName, string resetUrl, int expiryHours = 24)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Password Reset</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #ff6f00; color: white; padding: 20px; text-align: center;'>
        <h1>Password Reset Request</h1>
    </div>
    <div style='padding: 20px; background-color: #f5f5f5;'>
        <p>Hello {userName},</p>
        <p>We received a request to reset your password. Click the button below to create a new password:</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{resetUrl}' style='background-color: #ff6f00; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px;'>Reset Password</a>
        </p>
        <p>This link will expire in {expiryHours} hours.</p>
        <p>If you didn't request a password reset, please ignore this email or contact us if you have concerns.</p>
        <p>Warm regards,<br>The GiveAID Team</p>
    </div>
</body>
</html>";
    }

    public static string GetInvitationTemplate(string inviterName, string inviteeName, string invitationUrl, string? personalMessage = null)
    {
        var messageSection = string.IsNullOrEmpty(personalMessage)
            ? ""
            : $@"<p style='font-style: italic; color: #666;'>""{personalMessage}""</p>";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>You're Invited!</title>
</head>
<body style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;'>
    <div style='background-color: #7b1fa2; color: white; padding: 20px; text-align: center;'>
        <h1>You're Invited to Join GiveAID!</h1>
    </div>
    <div style='padding: 20px; background-color: #f5f5f5;'>
        <p>Hello {inviteeName},</p>
        <p>{inviterName} thinks you'd be a great fit for GiveAID and has invited you to join!</p>
        {messageSection}
        <p>GiveAID is a platform where people come together to support causes that matter and create positive change in our communities.</p>
        <p style='text-align: center; margin: 30px 0;'>
            <a href='{invitationUrl}' style='background-color: #7b1fa2; color: white; padding: 12px 24px; text-decoration: none; border-radius: 5px;'>Accept Invitation</a>
        </p>
        <p>Warm regards,<br>The GiveAID Team</p>
    </div>
</body>
</html>";
    }
}
