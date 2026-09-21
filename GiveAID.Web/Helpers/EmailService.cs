using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using GiveAID.Web.Data;
using GiveAID.Web.Models;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Centralised outbound email sender for Care4Kids.
    ///
    /// ## Mock mode
    /// When <c>SmtpEnabled=false</c> (the default), emails are logged to the
    /// <c>EmailLogs</c> table with Status="MockSent" and nothing is sent over the
    /// wire. All callers continue to work normally; admins can inspect the log
    /// table to verify that the right emails would have been delivered.
    ///
    /// ## Real SMTP
    /// Set <c>SmtpEnabled=true</c> and supply host/port/credentials in
    /// Web.config. The same <c>Send()</c> and typed helper methods work for both
    /// modes — no caller code changes are required.
    ///
    /// ## Retry
    /// Failed emails accumulate in the table with Status="Failed". Call the
    /// static <see cref="RetryFailedEmails(int)"/> method manually or from a
    /// scheduled background task to re-attempt them.
    ///
    /// ## Security
    /// Never commit real SMTP credentials to source control. Use Web.config
    /// environment-variable substitution or transformation files for production.
    /// </summary>
    public static class EmailService
    {
        // ── Brand colours ──────────────────────────────────────────────────
        private const string BRAND_CORAL = "#E87A5A";
        private const string BRAND_TEAL   = "#0E7490";
        private const string BRAND_ORG    = "Care4Kids";

        // ── Configuration helpers ─────────────────────────────────────────

        private static string AppSetting(string key, string fallback)
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[key]
                       ?? fallback;
            }
            catch { return fallback; }
        }

        /// <summary>Set SmtpEnabled=true in Web.config to enable real SMTP delivery.</summary>
        public static bool SmtpEnabled
        {
            get => string.Equals(
                AppSetting("SmtpEnabled", "false"),
                "true", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>SMTP host — e.g. smtp.gmail.com, smtp.sendgrid.net.</summary>
        public static string SmtpHost => AppSetting("SmtpHost", "localhost");

        /// <summary>SMTP port — typically 587 (submission) or 465 (implicit SSL).</summary>
        public static int SmtpPort
        {
            get
            {
                int p; return int.TryParse(AppSetting("SmtpPort", "587"), out p) ? p : 587;
            }
        }

        /// <summary>
        /// Username / full email address for SMTP authentication.
        /// For SendGrid use the string "apikey" and set SmtpPassword to your API key.
        /// </summary>
        public static string SmtpUsername => AppSetting("SmtpUsername", "");

        /// <summary>
        /// Password (or API key for SendGrid) for SMTP authentication.
        /// IMPORTANT: never commit this to source control.
        /// </summary>
        public static string SmtpPassword => AppSetting("SmtpPassword", "");

        /// <summary>
        /// Whether to use implicit TLS/SSL on port 465 (true) or STARTTLS on 587 (false).
        /// Defaults to true for safety.
        /// </summary>
        public static bool SmtpUseSsl
        {
            get => !string.Equals(
                AppSetting("SmtpUseSsl", "true"),
                "false", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>The From: address used in all outbound emails.</summary>
        public static string FromAddress => AppSetting("SmtpFrom", "no-reply@care4kids.org");

        /// <summary>Human-readable sender name used in the From: header.</summary>
        public static string FromName => AppSetting("SmtpFromName", BRAND_ORG);

        /// <summary>Base URL of the public site — used to build absolute links.</summary>
        public static string PublicSiteUrl => AppSetting("PublicSiteUrl", "https://care4kids.org");

        // ── Result type ────────────────────────────────────────────────────

        public class SendResult
        {
            public bool Success { get; set; }
            public string Error { get; set; }
            public int? EmailLogId { get; set; }
        }

        // ── Core send ─────────────────────────────────────────────────────

        /// <summary>
        /// Generic send — writes to EmailLog BEFORE attempting delivery, then
        /// updates Status to Sent|Failed|MockSent after.
        /// </summary>
        public static SendResult Send(
            string toEmail,
            string subject,
            string body,
            string category = "general",
            int? relatedId = null)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                return new SendResult { Success = false, Error = "toEmail is required." };

            // Always write a log row first (before any network call).
            int logId;
            using (var ctx = new GiveAIDContext())
            {
                var log = new EmailLog
                {
                    ToEmail    = toEmail.Trim(),
                    Subject    = subject ?? "",
                    Body       = body ?? "",
                    Category   = category,
                    RelatedId  = relatedId,
                    Status     = SmtpEnabled ? "Pending" : "MockSent",
                    CreatedAt  = DateTime.UtcNow
                };
                ctx.EmailLogs.Add(log);
                ctx.SaveChanges();
                logId = log.EmailLogId;
            }

            if (!SmtpEnabled)
            {
                // Mock mode — nothing to do; row is already persisted.
                System.Diagnostics.Debug.WriteLine(
                    $"[EmailService MOCK] id={logId} to={toEmail} subject={subject}");
                return new SendResult { Success = true, EmailLogId = logId };
            }

            // Real SMTP path.
            try
            {
                using (var msg = new MailMessage())
                {
                    msg.From       = new MailAddress(FromAddress, FromName);
                    msg.To.Add(toEmail.Trim());
                    msg.Subject    = subject ?? "";
                    msg.Body       = body ?? "";
                    msg.IsBodyHtml = true;

                    using (var client = new SmtpClient(SmtpHost, SmtpPort))
                    {
                        client.EnableSsl = SmtpUseSsl;
                        client.DeliveryMethod = SmtpDeliveryMethod.Network;

                        if (!string.IsNullOrWhiteSpace(SmtpUsername))
                        {
                            client.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                        }
                        // else: use default credentials (integrated auth — works in some corp SMTP setups)

                        client.Send(msg);
                    }
                }

                UpdateLogStatus(logId, "Sent", null);
                return new SendResult { Success = true, EmailLogId = logId };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[EmailService ERROR] id={logId} {ex}");
                UpdateLogStatus(logId, "Failed", ex.Message);
                return new SendResult { Success = false, Error = ex.Message, EmailLogId = logId };
            }
        }

        private static void UpdateLogStatus(int logId, string status, string error)
        {
            try
            {
                using (var ctx = new GiveAIDContext())
                {
                    var log = ctx.EmailLogs.Find(logId);
                    if (log == null) return;
                    log.Status = status;
                    if (status == "Sent") log.SentAt = DateTime.UtcNow;
                    if (error != null)
                    {
                        log.ErrorMessage = error.Length > 2000
                            ? error.Substring(0, 2000)
                            : error;
                        log.RetryCount++;
                    }
                    ctx.SaveChanges();
                }
            }
            catch { /* never let a logging failure propagate */ }
        }

        // ── Retry ─────────────────────────────────────────────────────────

        /// <summary>
        /// Re-attempts every EmailLog row whose Status is "Failed" or
        /// "Pending" (stale).  Returns a summary of what was requeued.
        /// Call this from a scheduled endpoint or background worker.
        /// </summary>
        /// <param name="maxAttempts">Rows with RetryCount >= this value are skipped.</param>
        public static int RetryFailedEmails(int maxAttempts = 3)
        {
            int retried = 0;
            try
            {
                using (var ctx = new GiveAIDContext())
                {
                    var candidates = ctx.EmailLogs
                        .Where(l =>
                            (l.Status == "Failed" || l.Status == "Pending") &&
                            l.RetryCount < maxAttempts)
                        .ToList();

                    foreach (var log in candidates)
                    {
                        try
                        {
                            using (var msg = new MailMessage())
                            {
                                msg.From       = new MailAddress(FromAddress, FromName);
                                msg.To.Add(log.ToEmail);
                                msg.Subject    = log.Subject;
                                msg.Body       = log.Body;
                                msg.IsBodyHtml = true;

                                using (var client = new SmtpClient(SmtpHost, SmtpPort))
                                {
                                    client.EnableSsl = SmtpUseSsl;
                                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                                    if (!string.IsNullOrWhiteSpace(SmtpUsername))
                                        client.Credentials = new NetworkCredential(SmtpUsername, SmtpPassword);
                                    client.Send(msg);
                                }
                            }

                            log.Status = "Sent";
                            log.SentAt = DateTime.UtcNow;
                        }
                        catch (Exception ex)
                        {
                            log.Status = "Failed";
                            log.ErrorMessage = ex.Message?.Length > 2000
                                ? ex.Message.Substring(0, 2000)
                                : ex.Message;
                            log.RetryCount++;
                        }
                        retried++;
                    }
                    ctx.SaveChanges();
                }
            }
            catch { /* swallow — caller can inspect the table directly */ }

            return retried;
        }

        // ── Typed helpers ─────────────────────────────────────────────────

        /// <summary>
        /// Sends an invitation email to the invitee, with a link that calls
        /// /api/invitations/accept/{token}.
        /// </summary>
        public static SendResult SendInvitation(Invitation inv, User inviter)
        {
            var inviterName = inviter?.FullName ?? "A friend";
            var acceptUrl   = $"{PublicSiteUrl}/invite/{inv.InvitationToken}";

            var subject = $"{inviterName} invited you to join {BRAND_ORG}";

            var body = BuildHtmlEmail(
                subject,
                $@"<p style=""margin:0 0 16px"">
  <span style=""font-size:18px; font-weight:600"">You're invited!</span>
</p>
<p style=""margin:0 0 16px"">
  <strong>{System.Web.HttpUtility.HtmlEncode(inviterName)}</strong> thinks
  <strong>{BRAND_ORG}</strong> — a community-driven charitable platform —
  would be a great fit for you.
</p>
" +
                (!string.IsNullOrWhiteSpace(inv.PersonalMessage)
                    ? $@"<blockquote style=""margin:16px 0; padding:12px 16px;
                        border-left:4px solid {BRAND_CORAL};
                        background:#FEF7F5; color:#444; font-style:italic"">
  {System.Web.HttpUtility.HtmlEncode(inv.PersonalMessage)}
</blockquote>"
                    : "") +
$@"<p style=""margin:24px 0"">
  <a href=""{acceptUrl}"" style=""display:inline-block;
    background:{BRAND_CORAL}; color:#fff; text-decoration:none;
    padding:12px 28px; border-radius:6px; font-weight:600"">
    Accept Invitation
  </a>
</p>
<p style=""margin:0 0 8px; color:#888; font-size:13px"">
  Or copy and paste this link into your browser:
</p>
<p style=""margin:0; font-size:13px; word-break:break-all"">
  <a href=""{acceptUrl}"" style=""color:{BRAND_TEAL}"">{acceptUrl}</a>
</p>");

            return Send(inv.InviteeEmail, subject, body, "invitation", inv.InvitationId);
        }

        /// <summary>
        /// Sends a donation receipt after a confirmed (Completed) donation.
        /// </summary>
        public static SendResult SendDonationReceipt(Donation donation, User donor)
        {
            var donorName = donor?.FullName ?? "Supporter";
            var amount    = donation.Amount.ToString("N0");
            var date      = (donation.PaymentConfirmedAt ?? donation.DonationDate)
                                .ToString("MMMM dd, yyyy 'at' HH:mm");
            var receiptNo = $"RCP-{donation.DonationId:D6}";
            var causeName = donation.Cause?.CauseName ?? "our cause";
            var campaign  = donation.Campaign?.CampaignName;

            var subject = $"[{receiptNo}] Thank you for your {amount} VND donation to {BRAND_ORG}!";

            // Build the receipt table rows.
            var tableRows = string.Concat(
                $@"<tr>
  <td style=""padding:4px 0; color:#555"">Cause</td>
  <td style=""padding:4px 0; font-weight:600; text-align:right"">
    {System.Web.HttpUtility.HtmlEncode(causeName)}
  </td>
</tr>",
                campaign != null
                    ? $@"<tr>
  <td style=""padding:4px 0; color:#555"">Campaign</td>
  <td style=""padding:4px 0; font-weight:600; text-align:right"">
    {System.Web.HttpUtility.HtmlEncode(campaign)}
  </td>
</tr>"
                    : "",
                $@"<tr>
  <td style=""padding:4px 0; color:#555"">Amount</td>
  <td style=""padding:4px 0; font-weight:700; font-size:20px; text-align:right; color:{BRAND_TEAL}"">
    {amount} VND
  </td>
</tr>",
                @"<tr>
  <td style=""padding:4px 0; color:#555"">Method</td>
  <td style=""padding:4px 0; text-align:right"">
    " + System.Web.HttpUtility.HtmlEncode(donation.PaymentMethod ?? "—") + @"
  </td>
</tr>",
                !string.IsNullOrWhiteSpace(donation.TransactionId)
                    ? @"<tr>
  <td style=""padding:4px 0; color:#555"">Reference</td>
  <td style=""padding:4px 0; text-align:right; font-family:monospace; font-size:13px"">
    " + System.Web.HttpUtility.HtmlEncode(donation.TransactionId) + @"
  </td>
</tr>"
                    : "");

            // Build the CTA button — must be a separate verbatim string because
            // the interpolated href + concatenated style is not valid inside a
            // single interpolated verbatim literal.
            var ctaButton = $@"<p style=""margin:24px 0 0; text-align:center"">
  <a href=""{PublicSiteUrl}"" style=""display:inline-block;
    background:{BRAND_CORAL}; color:#fff; text-decoration:none;
    padding:10px 24px; border-radius:6px; font-weight:600"">
    View Your Impact
  </a>
</p>";

            var body = BuildHtmlEmail(
                subject,
$@"<p style=""margin:0 0 6px; font-size:14px; color:#888"">
  {receiptNo} &nbsp;&middot;&nbsp; {date}
</p>
<h2 style=""margin:0 0 20px; color:{BRAND_CORAL}"">
  Thank you, {System.Web.HttpUtility.HtmlEncode(donorName)}!
</h2>
<div style=""background:#F0F9FB; border-radius:8px; padding:20px; margin:0 0 24px"">
  <table style=""width:100%; border-collapse:collapse; font-size:15px"">
{tableRows}
  </table>
</div>
<p style=""margin:0 0 8px; color:#555; font-size:14px"">
  Your donation has been received and will go directly toward supporting
  <strong>{System.Web.HttpUtility.HtmlEncode(causeName)}</strong>.
</p>
<p style=""margin:0 0 24px; color:#888; font-size:13px"">
  If you have any questions, please reply to this email or visit our contact page.
</p>
{tableRows}
{ctaButton}");

            return Send(donor?.Email ?? "", subject, body, "donation_receipt", donation.DonationId);
        }

        /// <summary>
        /// Sends a programme/campaign registration confirmation to the registrant.
        /// </summary>
        public static SendResult SendRegistrationConfirmation(
            CampaignRegistration registration,
            User user,
            Campaign campaign)
        {
            var userName     = user?.FullName ?? "Supporter";
            var campaignName = campaign?.CampaignName ?? "this programme";
            var startDate    = campaign != null ? campaign.StartDate.ToString("MMMM dd, yyyy") : "TBD";
            var location     = campaign?.Location;

            var subject = $"[{BRAND_ORG}] You're registered — {campaignName}";

            var body = BuildHtmlEmail(
                subject,
$@"<h2 style=""margin:0 0 16px; color:{BRAND_CORAL}"">
  Registration Confirmed!
</h2>
<p style=""margin:0 0 16px; font-size:15px; color:#333"">
  Hi <strong>{System.Web.HttpUtility.HtmlEncode(userName)}</strong>,
</p>
<p style=""margin:0 0 16px; color:#555"">
  We're delighted to confirm your registration for:
</p>
<div style=""background:#F0F9FB; border-radius:8px; padding:20px; margin:0 0 24px"">
  <div style=""font-size:18px; font-weight:700; color:{BRAND_TEAL}; margin:0 0 8px"">
    {System.Web.HttpUtility.HtmlEncode(campaignName)}
  </div>
" +
(location != null
    ? $@"  <div style=""color:#555; margin:4px 0"">
      📍 {System.Web.HttpUtility.HtmlEncode(location)}
    </div>"
    : "") +
$@"  <div style=""color:#555; margin:4px 0"">
    📅 {startDate}
  </div>
  <div style=""color:#555; margin:4px 0"">
    🎟️ Status: Registered
  </div>
</div>
<p style=""margin:0 0 24px; color:#555; font-size:14px"">
  We'll send you a reminder closer to the start date with everything you need to know.
  Thank you for joining us in making a difference!
</p>
  <p style=""margin:24px 0 0; text-align:center"">
  <a href=""{PublicSiteUrl}"" style=""display:inline-block;
    background:{BRAND_CORAL}; color:#fff; text-decoration:none;
    padding:10px 24px; border-radius:6px; font-weight:600"">
    Visit {BRAND_ORG}
  </a>
</p>");

            return Send(user?.Email ?? "", subject, body, "registration_confirmation", registration.RegistrationId);
        }

        /// <summary>
        /// Sends a reply to a contact-form submission from a site visitor.
        /// </summary>
        public static SendResult SendContactReply(ContactMessage message)
        {
            var subject = $"Re: [{BRAND_ORG}] {message.Subject ?? "Your message"}";
            var body = BuildHtmlEmail(
                subject,
$@"<p style=""margin:0 0 16px; font-size:15px; color:#333"">
  Dear <strong>{System.Web.HttpUtility.HtmlEncode(message.Name)}</strong>,
</p>
<div style=""background:#FEF9F7; border-left:4px solid {BRAND_CORAL};
  padding:16px; margin:0 0 20px; color:#444; white-space:pre-wrap"">
  {System.Web.HttpUtility.HtmlEncode(message.ReplyMessage ?? "")}
</div>
<p style=""margin:0 0 12px; color:#555; font-size:14px"">
  Your original message:
</p>
<div style=""color:#888; font-size:13px; white-space:pre-wrap; border-top:1px solid #eee; padding-top:12px"">
  {System.Web.HttpUtility.HtmlEncode(message.Message ?? "")}
</div>
<p style=""margin:24px 0 0; color:#888; font-size:13px"">
  — The {BRAND_ORG} Team &nbsp;|&nbsp; <a href=""{PublicSiteUrl}"" style=""color:{BRAND_TEAL}"">{PublicSiteUrl}</a>
</p>");

            return Send(message.Email, subject, body, "contact_reply", message.ContactId);
        }

        /// <summary>
        /// Sends a password reset email with a secure reset link.
        /// </summary>
        public static void SendPasswordResetEmail(string toEmail, string userName, string resetLink)
        {
            var subject = $"Reset Your {BRAND_ORG} Password";

            var body = BuildHtmlEmail(
                subject,
$@"<h2 style=""margin:0 0 16px; color:{BRAND_CORAL}"">
  Password Reset Request
</h2>
<p style=""margin:0 0 16px; font-size:15px; color:#333"">
  Hello <strong>{System.Web.HttpUtility.HtmlEncode(userName ?? "there")}</strong>,
</p>
<p style=""margin:0 0 20px; color:#555"">
  You requested a password reset for your <strong>{BRAND_ORG}</strong> account.
</p>
<p style=""margin:0 0 8px; color:#555"">
  Click the button below to reset your password:
</p>
<p style=""margin:24px 0"">
  <a href=""{resetLink}"" style=""display:inline-block;
    background:{BRAND_CORAL}; color:#fff; text-decoration:none;
    padding:12px 28px; border-radius:6px; font-weight:600; font-size:15px"">
    Reset Password
  </a>
</p>
<p style=""margin:16px 0 8px; color:#555; font-size:14px"">
  Or copy and paste this link into your browser:
</p>
<p style=""margin:0 0 24px; font-size:13px; word-break:break-all"">
  <a href=""{resetLink}"" style=""color:{BRAND_TEAL}"">{resetLink}</a>
</p>
<div style=""background:#FEF9F7; border-radius:6px; padding:12px 16px; margin:0 0 24px"">
  <p style=""margin:0; font-size:13px; color:#666"">
    ⏰ This link expires in <strong>1 hour</strong>.
  </p>
</div>
<p style=""margin:0 0 16px; color:#888; font-size:13px"">
  If you didn't request this password reset, please ignore this email.
  Your password will remain unchanged.
</p>
<p style=""margin:24px 0 0; color:#888; font-size:13px"">
  — The {BRAND_ORG} Team &nbsp;|&nbsp; <a href=""{PublicSiteUrl}"" style=""color:{BRAND_TEAL}"">{PublicSiteUrl}</a>
</p>");

            Send(toEmail, subject, body, "password_reset");
        }

        // ── HTML template ─────────────────────────────────────────────────

        /// <summary>
        /// Wraps the provided HTML content in the Care4Kids branded email shell.
        /// </summary>
        private static string BuildHtmlEmail(string subject, string contentHtml)
        {
            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>{System.Web.HttpUtility.HtmlEncode(subject)}</title>
</head>
<body style=""margin:0; padding:0;
  background:#F4F4F4; font-family:'Segoe UI', Arial, sans-serif"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0""
    style=""background:#F4F4F4; padding:30px 16px"">
    <tr>
      <td align=""center"">
        <table width=""600"" cellpadding=""0"" cellspacing=""0""
          style=""background:#ffffff; border-radius:10px;
                 overflow:hidden; max-width:600px; width:100%;
                 box-shadow:0 4px 20px rgba(0,0,0,0.08)"" >
          <!-- Header -->
          <tr>
            <td style=""background:{BRAND_CORAL}; padding:24px 32px;
                       text-align:center"">
              <div style=""font-size:22px; font-weight:700;
                          color:#fff; letter-spacing:0.5px"">
                {BRAND_ORG}
              </div>
              <div style=""font-size:12px; color:rgba(255,255,255,0.8);
                          margin-top:4px"">
                Children's Welfare &amp; Donation Platform
              </div>
            </td>
          </tr>
          <!-- Body -->
          <tr>
            <td style=""padding:32px 36px 24px; color:#222"">
{contentHtml}
            </td>
          </tr>
          <!-- Footer -->
          <tr>
            <td style=""background:#F9F9F9; padding:16px 32px;
                       border-top:1px solid #EDEDED"">
              <p style=""margin:0; font-size:12px; color:#AAA;
                        text-align:center"">
                © {DateTime.UtcNow.Year} {BRAND_ORG}. All rights reserved.<br />
                This email was sent because you have an account or made a donation
                on our platform.<br />
                <a href=""{PublicSiteUrl}"" style=""color:{BRAND_TEAL}"">{PublicSiteUrl}</a>
              </p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
