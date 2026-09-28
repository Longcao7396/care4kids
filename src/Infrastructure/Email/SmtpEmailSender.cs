using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using GiveAID.Application.Services;
using Microsoft.Extensions.Logging;
using GiveAID.Infrastructure.Email;

namespace GiveAID.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;
    private readonly IEmailLogService _emailLogService;
    private readonly ILogger<SmtpEmailSender> _logger;
    
    // M-14: Retry configuration - use settings if available, otherwise defaults
    private int MaxRetries => _settings.MaxRetries > 0 ? _settings.MaxRetries : 3;
    private TimeSpan BaseRetryDelay => TimeSpan.FromSeconds(_settings.RetryDelaySeconds > 0 ? _settings.RetryDelaySeconds : 2);

    public SmtpEmailSender(
        IOptions<SmtpSettings> settings,
        IEmailLogService emailLogService,
        ILogger<SmtpEmailSender> logger)
    {
        _settings = settings.Value;
        _emailLogService = emailLogService;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
    {
        // If SMTP is disabled, log as mock sent and return success
        if (!_settings.SmtpEnabled)
        {
            _logger.LogInformation("SMTP disabled - logging email as mock sent to {To}: {Subject}", to, subject);
            await _emailLogService.LogEmailAsync(to, subject, body, success: true, errorMessage: null);
            return true;
        }

        // M-14: Retry loop with exponential backoff
        for (int attempt = 1; attempt <= MaxRetries; attempt++)
        {
            try
            {
                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(_settings.SmtpUsername, _settings.SmtpPassword)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_settings.FromEmail, _settings.FromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation("Email sent successfully to {To}: {Subject}", to, subject);
                await _emailLogService.LogEmailAsync(to, subject, body, success: true, errorMessage: null);
                return true;
            }
            catch (SmtpException ex) when (attempt < MaxRetries)
            {
                // M-14: Log warning and retry with exponential backoff
                var delay = BaseRetryDelay * (int)Math.Pow(2, attempt - 1);
                _logger.LogWarning(ex, 
                    "Email send attempt {Attempt}/{MaxRetries} failed for {To}: {Subject}. Retrying in {Delay}s...",
                    attempt, MaxRetries, to, subject, delay.TotalSeconds);
                await Task.Delay(delay);
            }
            catch (SmtpException ex)
            {
                // M-14: All retries exhausted - log error and bubble up
                _logger.LogError(ex, "Email send failed after {MaxRetries} attempts for {To}: {Subject}", 
                    MaxRetries, to, subject);
                await _emailLogService.LogEmailAsync(to, subject, body, success: false, errorMessage: 
                    $"Failed after {MaxRetries} attempts: {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                // M-14: Non-SMTP exceptions - log error and bubble up
                _logger.LogError(ex, "Unexpected error sending email to {To}: {Subject}", to, subject);
                await _emailLogService.LogEmailAsync(to, subject, body, success: false, errorMessage: ex.Message);
                return false;
            }
        }

        return false; // Should not reach here
    }

    public async Task<bool> SendPasswordResetEmailAsync(string email, string resetToken, string resetUrl)
    {
        var subject = "Password Reset - GiveAID";
        // L-01: Use the proper HTML template with correct URL (resetUrl already contains the token)
        var body = EmailTemplates.GetPasswordResetTemplate(
            userName: email.Split('@')[0],
            resetUrl: resetUrl,
            expiryHours: 1);
        return await SendEmailAsync(email, subject, body, isHtml: true);
    }

    public async Task<bool> SendDonationReceiptAsync(string email, int donationId, decimal amount, string campaignName)
    {
        var subject = $"Donation Receipt - GiveAID";
        var body = $@"
            <h2>Thank you for your donation!</h2>
            <p>Your donation has been received.</p>
            <p><strong>Donation ID:</strong> {donationId}</p>
            <p><strong>Campaign:</strong> {campaignName}</p>
            <p><strong>Amount:</strong> {amount:C}</p>
            <p>Thank you for your generosity!</p>
        ";
        return await SendEmailAsync(email, subject, body, isHtml: true);
    }

    public async Task<bool> SendRegistrationConfirmationAsync(string email, string username, string verificationToken)
    {
        var subject = "Welcome to GiveAID!";
        var loginUrl = $"{_settings.PublicSiteUrl ?? "https://giveaid.org"}/login";
        var body = EmailTemplates.GetWelcomeEmailTemplate(username, loginUrl);
        return await SendEmailAsync(email, subject, body, isHtml: true);
    }
}
