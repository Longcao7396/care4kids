using System.Web.Http;
using System.Web.Http.Cors;

namespace GiveAID.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // SECURITY: CORS allowlist is now resolved once in Global.asax and
            // shared with Application_BeginRequest. Previously this file had its
            // own hardcoded dev list AND a parallel echo-anywhere code path in
            // Global.asax, which together allowed credentialed cross-origin from
            // arbitrary sites (CSRF). Both layers now use the validated allowlist.
            var allowedOrigins = WebApiApplication.CorsAllowedOrigins;
            var cors = new EnableCorsAttribute(
                origins: string.Join(",", allowedOrigins),
                headers: "*",
                methods: "*");
            cors.SupportsCredentials = true;
            cors.ExposedHeaders.Add("Authorization");
            config.EnableCors(cors);

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
