namespace GiveAID.Application.Services;

/// <summary>
/// Interface for sending emails.
/// Implementation provided by Infrastructure layer.
/// </summary>
public interface IEmailSender
{
    /// <summary>
    /// Sends an email to the specified recipient.
    /// </summary>
    Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);

    /// <summary>
    /// Sends a password reset email with the reset link.
    /// </summary>
    Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl);

    /// <summary>
    /// Sends a donation receipt email.
    /// </summary>
    Task<bool> SendDonationReceiptAsync(string email, int donationId, decimal amount, string campaignName);

    /// <summary>
    /// Sends a registration confirmation email.
    /// </summary>
    Task<bool> SendRegistrationConfirmationAsync(string email, string username, string verificationToken);
}
