using System.Reflection;
using System.Web.Http;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Minimal health/status endpoints for the GiveAID Web API backend.
    /// Replaces the legacy MVC catch-all route that used to serve a Home/Index view.
    /// This is an API-only project; all real functionality lives under /api/...
    /// </summary>
    [RoutePrefix("")]
    public class HealthController : ApiController
    {
        /// <summary>
        /// GET /  — service banner. Returns JSON describing the backend.
        /// </summary>
        [HttpGet]
        [Route("")]
        public IHttpActionResult Index()
        {
            return Ok(new
            {
                service = "GiveAID Web API",
                status = "running",
                version = Assembly.GetExecutingAssembly().GetName().Version.ToString(),
                framework = ".NET Framework " + Assembly.GetExecutingAssembly().ImageRuntimeVersion,
                endpoints = "All API endpoints are exposed under /api/{controller}/{action}",
                health = Url.Content("~/health"),
                frontend = "http://localhost:3000",
                cors = "CORS is configured to allow http://localhost:3000",
                timestamp = System.DateTime.UtcNow.ToString("o")
            });
        }

        /// <summary>
        /// GET /health — lightweight health probe.
        /// </summary>
        [HttpGet]
        [Route("health")]
        public IHttpActionResult Health()
        {
            return Ok(new
            {
                status = "healthy",
                timestamp = System.DateTime.UtcNow.ToString("o")
            });
        }
    }
}
