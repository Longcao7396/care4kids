using System.Web.Http;
using System.Web.Http.Cors;
using GiveAID.Web.Helpers;

namespace GiveAID.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // SECURITY: Global exception handler - returns JSON instead of HTML error pages
            config.Filters.Add(new ApiExceptionFilterAttribute());
            // SECURITY: CORS allowlist is now resolved once in Global.asax and
            // shared with Application_BeginRequest. Previously this file had its
            // own hardcoded dev list AND a parallel echo-anywhere code path in
            // Global.asax, which together allowed credentialed cross-origin from
            // arbitrary sites (CSRF). Both layers now use the validated allowlist.
            // SECURITY: For non-OPTIONS requests we let Web API's own CORS
            // pipeline add a single Access-Control-Allow-Origin (from the
            // validated allowlist) once. Application_BeginRequest in
            // Global.asax.cs handles OPTIONS preflight separately.
            // We deliberately do NOT call config.EnableCors(...) here to avoid
            // emitting duplicate CORS headers when both layers respond to
            // the same request.
            var allowedOrigins = WebApiApplication.CorsAllowedOrigins;
            // Touch the allowlist so it is resolved at startup (matches the
            // eager-init in Global.asax and surfaces misconfig early).
            _ = allowedOrigins;

            // Web API routes
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // JSON formatter settings
            var json = config.Formatters.JsonFormatter;
            json.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
            json.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss";
            // Force UTF-8 so non-ASCII characters (Vietnamese etc.) survive serialization
            json.SupportedEncodings.Clear();
            json.SupportedEncodings.Add(new System.Text.UTF8Encoding(false, true));
            json.SupportedMediaTypes.Clear();
            json.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("application/json"));
            json.SupportedMediaTypes.Add(new System.Net.Http.Headers.MediaTypeHeaderValue("text/json"));

            // Remove XML formatter
            config.Formatters.Remove(config.Formatters.XmlFormatter);
        }
    }
}
