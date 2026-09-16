using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Idempotent bootstrap endpoint:
    ///   POST /api/auth/bootstrap
    /// Re-hashes admin and demo users with correctly-formatted BCrypt hashes
    /// (cost 11) for the two known default passwords. Safe to call repeatedly.
    /// Used to recover from any desynchronization between DB and the new
    /// password verification path.
    ///
    /// SECURITY: Caller must present a valid SuperAdmin JWT. We can't use
    /// [JwtAuthorize] because OWIN bearer middleware isn't wired up; instead
    /// we validate the JWT manually via JwtHelper and check the role claim.
    /// </summary>
    [RoutePrefix("api/auth")]
    public class AuthBootstrapController : ApiController
    {
        // SECURITY: This endpoint rewrites admin/demo account hashes.
        // We cannot use [Authorize(Roles="SuperAdmin")] because there is no
        // OAuth/OWIN bearer middleware configured; [Authorize] without that
        // middleware always returns 401. Instead, we manually validate the
        // JWT and check the role claim server-side.
        [HttpPost]
        [Route("bootstrap")]
        public IHttpActionResult Bootstrap()
        {
            // Verify caller has a SuperAdmin JWT.
            int callerId;
            try
            {
                callerId = JwtHelper.GetUserIdFromToken(Request);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            using (var ctx = new GiveAIDContext())
            {
                var caller = ctx.Users.Find(callerId);
                if (caller == null || caller.Role != "SuperAdmin" || !caller.IsActive)
                {
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "SuperAdmin role required." });
                }

                try
                {
                    // Safe to proceed — caller is SuperAdmin.
                    return DoBootstrap(ctx);
                }
                catch (Exception ex)
                {
                    return BadRequest("Bootstrap failed: " + ex.Message);
                }
            }
        }

        private IHttpActionResult DoBootstrap(GiveAIDContext ctx)
        {
                string adminHash = AuthBootstrap.Hash("Admin@123", 11);
                string userHash  = AuthBootstrap.Hash("User@123", 11);

                var admin = ctx.Users.FirstOrDefault(u => u.Email == "admin@give-aid.org");
                if (admin == null)
                {
                    admin = new User
                    {
                        Username = "admin",
                        Email = "admin@give-aid.org",
                        PasswordHash = adminHash,
                        FullName = "System Administrator",
                        Role = "SuperAdmin",
                        IsActive = true,
                        IsVerified = true,
                        CreatedAt = DateTime.Now
                    };
                    ctx.Users.Add(admin);
                }
                else
                {
                    admin.PasswordHash = adminHash;
                    admin.IsActive = true;
                }

                var demo = ctx.Users.FirstOrDefault(u => u.Email == "user@example.com");
                if (demo == null)
                {
                    demo = new User
                    {
                        Username = "demouser",
                        Email = "user@example.com",
                        PasswordHash = userHash,
                        FullName = "Demo User",
                        Role = "User",
                        IsActive = true,
                        IsVerified = true,
                        CreatedAt = DateTime.Now
                    };
                    ctx.Users.Add(demo);
                }
                else
                {
                    demo.PasswordHash = userHash;
                    demo.IsActive = true;
                }

                ctx.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Auth bootstrap applied successfully.",
                    data = new
                    {
                        adminEmail = admin.Email,
                        adminHashPrefix = admin.PasswordHash.Substring(0, 7),
                        demoEmail = demo.Email,
                        demoHashPrefix = demo.PasswordHash.Substring(0, 7)
                    }
                });
        }
    }
}
