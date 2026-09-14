using System;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using GiveAID.Web.Data;

namespace GiveAID.Web
{
    public class WebApiApplication : HttpApplication
    {
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

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Seed database — wrap in try/catch so the app still starts
            // if SQL Server is temporarily unreachable. Demo data is owned
            // by NGO_Database_Schema_V2.sql anyway.
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
            if (Request.HttpMethod == "OPTIONS")
            {
                var origin = Request.Headers["Origin"];
                if (!string.IsNullOrEmpty(origin))
                {
                    Response.AddHeader("Access-Control-Allow-Origin", origin);
                    Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE, OPTIONS, PATCH");
                    Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, X-Requested-With");
                    Response.AddHeader("Access-Control-Allow-Credentials", "true");
                    Response.AddHeader("Access-Control-Max-Age", "86400");
                }
                Response.StatusCode = 200;
                Response.End();
            }
        }

        protected void Application_Error()
        {
            // No-op — exceptions are handled per-controller. Empty body keeps
            // IIS from showing its generic error page when nothing else catches it.
            Server.ClearError();
        }
    }
}
