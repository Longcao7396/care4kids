using System.Configuration;
using System.Web.Http;
using System.Web.Http.Cors;

namespace GiveAID.Web
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // CORS: prefer the comma-separated `Cors:AllowedOrigins` appSetting
            // (production deployments). Fall back to the dev allowlist so the
            // project still runs out of the box. Must list specific origins
            // (cannot use "*") because axios sends withCredentials=true which
            // forbids wildcard Access-Control-Allow-Origin.
            var allowedOrigins = ConfigurationManager.AppSettings["Cors:AllowedOrigins"];
            if (string.IsNullOrWhiteSpace(allowedOrigins))
            {
                allowedOrigins = "http://localhost:3000, http://localhost:3001, http://localhost:3002, http://localhost:61508, http://localhost:44300, https://localhost:44300";
            }

            var cors = new EnableCorsAttribute(
                origins: allowedOrigins,
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
