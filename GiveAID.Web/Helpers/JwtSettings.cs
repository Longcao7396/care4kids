using System;
using System.Configuration;
using System.Web.Hosting;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Strongly-typed accessor for JWT configuration values from Web.config.
    /// Throws on missing keys so misconfiguration fails loudly at startup
    /// instead of silently producing tokens with a weak default secret.
    ///
    /// SECURITY: Each value is resolved in this priority order:
    ///   1. Environment variable (e.g. GIVEAID_JWT_SECRET) — recommended for
    ///      production deployments so the secret never lands in source control.
    ///   2. Web.config appSettings — fallback for local dev.
    ///
    /// Generate a strong secret with:
    ///   [Convert]::ToBase64String((New-Object Security.Cryptography.RNGCryptoServiceProvider)
    ///       .GetBytes(64))
    /// </summary>
    public static class JwtSettings
    {
        // Environment-variable fallbacks for each key. Lets ops inject the secret
        // via Azure App Settings / Kubernetes secrets / GitHub Actions without
        // ever touching Web.config.
        private static string ReadConfigOrEnv(string appSettingKey, string envVarName, bool required)
        {
            // 1. Environment variable wins.
            var fromEnv = Environment.GetEnvironmentVariable(envVarName);
            if (!string.IsNullOrWhiteSpace(fromEnv)) return fromEnv;

            // 2. Web.config fallback.
            var fromConfig = ConfigurationManager.AppSettings[appSettingKey];
            if (!string.IsNullOrWhiteSpace(fromConfig)) return fromConfig;

            if (required)
            {
                throw new InvalidOperationException(
                    $"JWT setting '{appSettingKey}' (env: {envVarName}) is not configured. " +
                    "Set it in Web.config appSettings OR as an environment variable.");
            }
            return null;
        }

        public static string Secret
        {
            get
            {
                var s = ReadConfigOrEnv("JwtSecret", "GIVEAID_JWT_SECRET", required: true);
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
                var s = ReadConfigOrEnv("JwtIssuer", "GIVEAID_JWT_ISSUER", required: true);
                return s;
            }
        }

        public static string Audience
        {
            get
            {
                var s = ReadConfigOrEnv("JwtAudience", "GIVEAID_JWT_AUDIENCE", required: true);
                return s;
            }
        }

        public static int ExpiryMinutes
        {
            get
            {
                var s = ReadConfigOrEnv("JwtExpiryMinutes", "GIVEAID_JWT_EXPIRY_MINUTES", required: true);
                int minutes;
                if (!int.TryParse(s, out minutes) || minutes <= 0)
                    throw new InvalidOperationException(
                        "JwtExpiryMinutes must be a positive integer (current: '" + s + "').");
                return minutes;
            }
        }
    }
}
