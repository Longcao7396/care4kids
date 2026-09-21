using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Filters;
using Newtonsoft.Json;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Global exception handler that returns JSON instead of HTML error pages.
    /// SECURITY: Prevents stack trace leakage in production responses.
    /// </summary>
    public class ApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext context)
        {
            // Log full exception details server-side
            System.Diagnostics.Trace.TraceError($"[API Exception] {context.Exception}");
            
            var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(new
                    {
                        success = false,
                        message = "An error occurred processing your request.",
                        // Only expose details in Debug mode
                        details = context.Exception.Message
                    }),
                    System.Text.Encoding.UTF8,
                    "application/json"
                )
            };
            
            context.Response = response;
        }
    }
}
