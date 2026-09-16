using System;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using GiveAID.Web.Data;

namespace GiveAID.Web
{
    public class WebApiApplication : HttpApplication
    {
        // SECURITY: Single source of truth for CORS allowlist. Used by both
        // WebApiConfig.Register (attribute-based CORS) AND Application_BeginRequest
        // (manual OPTIONS preflight). Previously the BeginRequest path echoed back
        // ANY Origin header value (CVE-grade CSRF). Now both layers share the
        // validated allowlist from Web.config `Cors:AllowedOrigins` (or dev fallback).
        private static string[] _corsAllowedOrigins;
        private static bool _corsIsDev;

        public static string[] CorsAllowedOrigins
        {
            get
            {
                if (_corsAllowedOrigins != null) return _corsAllowedOrigins;
                var raw = ConfigurationManager.AppSettings["Cors:AllowedOrigins"];
                if (string.IsNullOrWhiteSpace(raw))
                {
                    // SECURITY: In production we MUST NOT fall back to a localhost
                    // allowlist. If we're running in production and no allowlist
                    // is configured, that's a misconfiguration that should fail
                    // loudly at startup rather than silently allowing only dev
                    // origins (which would lock out real users while leaving the
                    // app running).
                    var env = (ConfigurationManager.AppSettings["Environment"]
                                ?? "Production").Trim();
                    if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase)
                        || string.Equals(env, "Dev", StringComparison.OrdinalIgnoreCase))
                    {
                        raw = "http://localhost:3000,http://localhost:3001,http://localhost:3002,http://localhost:61508,http://localhost:44300,https://localhost:44300";
                        _corsIsDev = true;
                        System.Diagnostics.Debug.WriteLine(
                            "[CORS] WARNING: using localhost-only dev allowlist because Cors:AllowedOrigins is empty and Environment=Development.");
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            "Cors:AllowedOrigins is required in non-Development environments. " +
                            "Set it in Web.config appSettings or as the Cors__AllowedOrigins " +
                            "environment variable (comma-separated list of allowed origins).");
                    }
                }
                _corsAllowedOrigins = raw.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim()).ToArray();
                return _corsAllowedOrigins;
            }
        }

        public static bool CorsIsDev => _corsIsDev;

        protected void Application_Start()
        {
            // Fail fast if JWT configuration is missing/invalid — much easier
            // to debug than discovering it at first authenticated request.
            try
            {
                var probe = GiveAID.Web.Helpers.JwtSettings.Secret;
                probe = GiveAID.Web.Helpers.JwtSettings.Issuer;
                probe = GiveAID.Web.Helpers.JwtSettings.Audience;
                var mins = GiveAID.Web.Helpers.JwtSettings.ExpiryMinutes;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    "JWT configuration is invalid. Check Web.config appSettings (JwtSecret, JwtIssuer, JwtAudience, JwtExpiryMinutes). Inner: " + ex.Message,
                    ex);
            }

            // Eagerly resolve the CORS allowlist at startup so misconfiguration
            // surfaces immediately (rather than at the first preflight request).
            var _ = CorsAllowedOrigins;

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Seed database — wrap in try/catch so the app still starts
            // if SQL Server is temporarily unreachable. Demo data is owned
            // by the SQL migration scripts (Campaigns_DataSeed.sql, etc.) anyway.
            try
            {
                using (var context = new GiveAIDContext())
                {
                    GiveAIDContext.SeedDatabase(context);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[Startup] SeedDatabase failed: " + ex.Message);
                // Swallow — the API controllers will return their own errors
                // per request when the DB is unavailable.
            }
        }

        protected void Application_BeginRequest()
        {
            // SECURITY: Manually handle only CORS preflight (OPTIONS) requests.
            // Previously this code echoed back ANY Origin header — a CSRF-grade
            // vulnerability because combined with Access-Control-Allow-Credentials:true
            // it allowed credentialed cross-origin reads from arbitrary sites.
            // Now we validate Origin against the same allowlist WebApiConfig uses.
            if (Request.HttpMethod != "OPTIONS") return;

            var origin = Request.Headers["Origin"];
            if (string.IsNullOrEmpty(origin)) return;

            // ONLY echo back the Origin if it's in the validated allowlist.
            if (!CorsAllowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
            {
                // Reject — do not add any CORS headers. Browser will block the request.
                Response.StatusCode = 403;
                Response.End();
                return;
            }

            Response.AddHeader("Access-Control-Allow-Origin", origin);
            Response.AddHeader("Vary", "Origin");
            Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");
            Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, X-Requested-With");
            Response.AddHeader("Access-Control-Allow-Credentials", "true");
            Response.AddHeader("Access-Control-Max-Age", "86400");
            Response.StatusCode = 200;
        }

        protected void Application_Error()
        {
            // No-op — exceptions are handled per-controller. Empty body keeps
            // IIS from showing its generic error page when nothing else catches it.
            Server.ClearError();
        }
    }
}
