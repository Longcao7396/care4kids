using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace GiveAID.Web.Helpers
{
    /// <summary>
    /// Custom [JwtAuthorize] attribute that validates the JWT bearer token
    /// directly without requiring OWIN/OAuth middleware. Replaces the stock
    /// [Authorize] attribute for endpoints that should require authentication.
    ///
    /// Usage:
    ///     [JwtAuthorize]                              — any authenticated user
    ///     [JwtAuthorize(Roles = "SuperAdmin,Admin")]  — role-gated
    ///
    /// Returns 401 when token missing/invalid; 403 when role mismatch.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class JwtAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            // Reuse base behaviour first (handles ClaimsPrincipal from auth pipeline).
            if (base.IsAuthorized(actionContext))
            {
                return true;
            }

            // Fall back: parse JWT from Authorization header ourselves.
            var principal = TryGetPrincipal(actionContext.Request);
            if (principal == null) return false;

            // Role check
            if (!string.IsNullOrEmpty(Roles))
            {
                var allowed = Roles.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(r => r.Trim());
                var userRole = principal.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (userRole == null || !allowed.Contains(userRole))
                {
                    return false;
                }
            }

            // Stash principal on the request so controllers can read it via
            // `this.User` without re-parsing the token.
            actionContext.RequestContext.Principal = principal;
            return true;
        }

        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            // Differentiate 401 (no/invalid token) vs 403 (token OK but role denied).
            var principal = TryGetPrincipal(actionContext.Request);
            if (principal == null)
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    System.Net.HttpStatusCode.Unauthorized,
                    new { success = false, message = "Authentication required." });
            }
            else
            {
                actionContext.Response = actionContext.Request.CreateResponse(
                    System.Net.HttpStatusCode.Forbidden,
                    new { success = false, message = "Insufficient role." });
            }
        }

        private static System.Security.Claims.ClaimsPrincipal TryGetPrincipal(HttpRequestMessage request)
        {
            try
            {
                var authHeader = request.Headers.Authorization;
                if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter)) return null;
                if (!string.Equals(authHeader.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase)) return null;
                return JwtHelper.ValidateToken(authHeader.Parameter);
            }
            catch
            {
                return null;
            }
        }
    }
}
