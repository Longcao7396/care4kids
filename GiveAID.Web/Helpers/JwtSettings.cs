using System;
using System.Configuration;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Strongly-typed accessor for JWT configuration values from Web.config.
    /// Throws on missing keys so misconfiguration fails loudly at startup
    /// instead of silently producing tokens with a weak default secret.
    /// </summary>
    public static class JwtSettings
    {
        public static string Secret
        {
            get
            {
                var s = ConfigurationManager.AppSettings["JwtSecret"];
                if (string.IsNullOrWhiteSpace(s))
                    throw new InvalidOperationException(
                        "JwtSecret is not configured in Web.config (appSettings).");
                if (s.Length < 32)
                    throw new InvalidOperationException(
                        "JwtSecret must be at least 32 characters long (current: " + s.Length + ").");
                return s;
            }
        }

        public static string Issuer
        {
            get
            {
                var s = ConfigurationManager.AppSettings["JwtIssuer"];
                if (string.IsNullOrWhiteSpace(s))
                    throw new InvalidOperationException("JwtIssuer is not configured in Web.config.");
                return s;
            }
        }

        public static string Audience
        {
            get
            {
                var s = ConfigurationManager.AppSettings["JwtAudience"];
                if (string.IsNullOrWhiteSpace(s))
                    throw new InvalidOperationException("JwtAudience is not configured in Web.config.");
                return s;
            }
        }

        public static int ExpiryMinutes
        {
            get
            {
                var s = ConfigurationManager.AppSettings["JwtExpiryMinutes"];
                int minutes;
                if (!int.TryParse(s, out minutes) || minutes <= 0)
                    throw new InvalidOperationException(
                        "JwtExpiryMinutes must be a positive integer (current: '" + s + "').");
                return minutes;
            }
        }
    }
}
