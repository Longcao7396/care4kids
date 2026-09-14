using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Authentication endpoints (login / me / logout).
    /// Minimal, single-responsibility controller — all complex password logic
    /// is delegated to <see cref="PasswordHasher"/>.
    /// </summary>
    [RoutePrefix("api/auth")]
    public class AuthController : ApiController
    {
        private readonly GiveAIDContext _context;

        public AuthController()
        {
            _context = new GiveAIDContext();
        }

        // POST: api/auth/login
        [HttpPost]
        [Route("login")]
        public IHttpActionResult Login(LoginRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Password))
                {
                    return BadRequest("Email and password are required.");
                }

                var email = request.Email.Trim();
                var user = _context.Users.FirstOrDefault(u => u.Email == email);

                if (user == null || !user.IsActive)
                {
                    return BadRequest("Invalid email or password.");
                }

                if (!PasswordHasher.Verify(request.Password, user.PasswordHash))
                {
                    return BadRequest("Invalid email or password.");
                }

                user.LastLogin = DateTime.Now;
                _context.SaveChanges();

                var token = JwtHelper.GenerateToken(user);

                return Ok(new
                {
                    success = true,
                    message = "Login successful",
                    data = new
                    {
                        token = token,
                        user = new
                        {
                            userId = user.UserId,
                            username = user.Username,
                            email = user.Email,
                            fullName = user.FullName,
                            role = user.Role,
                            profession = user.Profession,
                            phone = user.Phone,
                            isVerified = user.IsVerified
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AuthController.Login] " + ex);
                return BadRequest("Login failed: " + ex.Message);
            }
        }

        // GET: api/auth/me
        // NOTE: We do not use [Authorize] here because it requires OAuth bearer
        // middleware (UseOAuthBearerTokens / OWIN) which we are not running.
        // Instead we validate the JWT manually and reject if invalid.
        [HttpGet]
        [Route("me")]
        public IHttpActionResult Me()
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);
                var user = _context.Users.Find(userId);
                if (user == null) return NotFound();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        userId = user.UserId,
                        username = user.Username,
                        email = user.Email,
                        fullName = user.FullName,
                        role = user.Role,
                        profession = user.Profession,
                        phone = user.Phone,
                        address = user.Address,
                        isVerified = user.IsVerified
                    }
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[AuthController.Me] " + ex);
                return BadRequest(ex.Message);
            }
        }

        // POST: api/auth/logout (client-side: drop token; this just acknowledges)
        [HttpPost]
        [Route("logout")]
        public IHttpActionResult Logout()
        {
            // Require a valid JWT to acknowledge; otherwise return 401.
            try
            {
                JwtHelper.GetUserIdFromToken(Request);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            return Ok(new { success = true, message = "Logout successful" });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
