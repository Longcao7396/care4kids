using System;
using System.Net;
using System.Net.Mail;
using GiveAID.Web.Models;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Centralised outbound email sender.
    ///
    /// Currently runs in MOCK mode — it records that an email would have been
    /// sent and returns success, so the rest of the system (Invitations, etc.)
    /// can be exercised end-to-end without an SMTP provider configured.
    ///
    /// To enable real delivery: set <c>SmtpEnabled = true</c> in <c>Web.config</c>
    /// and add the SMTP host/port/credentials, then this helper alone changes —
    /// no controller code needs to be touched.
    /// </summary>
    public static class EmailService
    {
        /// <summary>Configuration flag — comes from Web.config key "SmtpEnabled".</summary>
        public static bool SmtpEnabled
        {
            get
            {
                try
                {
                    var raw = System.Configuration.ConfigurationManager.AppSettings["SmtpEnabled"];
                    return string.Equals(raw, "true", StringComparison.OrdinalIgnoreCase);
                }
                catch { return false; }
            }
        }

        public static string SmtpHost
        {
            get
            {
                try { return System.Configuration.ConfigurationManager.AppSettings["SmtpHost"] ?? "localhost"; }
                catch { return "localhost"; }
            }
        }

        public static int SmtpPort
        {
            get
            {
                try
                {
                    var raw = System.Configuration.ConfigurationManager.AppSettings["SmtpPort"];
                    int port; return int.TryParse(raw, out port) ? port : 25;
                }
                catch { return 25; }
            }
        }

        public static string FromAddress
        {
            get
            {
                try { return System.Configuration.ConfigurationManager.AppSettings["SmtpFrom"] ?? "no-reply@giveaid.org"; }
                catch { return "no-reply@giveaid.org"; }
            }
        }

        public static string PublicSiteUrl
        {
            get
            {
                try { return System.Configuration.ConfigurationManager.AppSettings["PublicSiteUrl"] ?? "https://giveaid.org"; }
                catch { return "https://giveaid.org"; }
            }
        }

        public class SendResult
        {
            public bool Success { get; set; }
            public string Error { get; set; }
        }

        /// <summary>
        /// Send an invitation email to the invitee, with a link that calls
        /// /api/invitations/accept/{token}.
        /// </summary>
        public static SendResult SendInvitation(Invitation inv, User inviter)
        {
            var inviterName = inviter?.FullName ?? "A friend";
            var subject = $"{inviterName} invited you to join Give-AID";

            var acceptUrl = $"{PublicSiteUrl}/invite/{inv.InvitationToken}";

            var body =
$@"Hi {inv.InviteeName},

{inviterName} thinks Give-AID — a community-driven non-profit platform — would be a great fit for you.

{System.Web.HttpUtility.HtmlEncode(inv.PersonalMessage ?? "")}

Join here: {acceptUrl}

— The Give-AID team
";

            return Send(inv.InviteeEmail, subject, body, "invitation", inv.InvitationId);
        }

        /// <summary>Generic sender — mock by default, real SMTP if SmtpEnabled.</summary>
        public static SendResult Send(string toEmail, string subject, string body,
            string category = "general", int? relatedId = null)
        {
            if (!SmtpEnabled)
            {
                // Mock mode — log to debug output so admins can verify it would have been sent.
                System.Diagnostics.Debug.WriteLine(
                    $"[EmailService MOCK] category={category} relatedId={relatedId} to={toEmail} subject={subject}");
                return new SendResult { Success = true, Error = null };
            }

            try
            {
                using (var msg = new MailMessage(FromAddress, toEmail))
                {
                    msg.Subject = subject;
                    msg.Body = body;
                    msg.IsBodyHtml = false;
                    using (var client = new SmtpClient(SmtpHost, SmtpPort))
                    {
                        client.EnableSsl = true;
                        client.Credentials = CredentialCache.DefaultNetworkCredentials;
                        client.Send(msg);
                    }
                }
                return new SendResult { Success = true };
            }
            catch (Exception ex)
            {
                return new SendResult { Success = false, Error = ex.Message };
            }
        }
    }
}
