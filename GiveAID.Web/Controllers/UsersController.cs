using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Self-service profile management for the authenticated user.
    /// All endpoints require a valid JWT — role is always the caller's own.
    /// </summary>
    [RoutePrefix("api/users")]
    [JwtAuthorize]
    public class UsersController : ApiController
    {
        private readonly GiveAIDContext _context;

        public UsersController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/users/me  (full profile, mirrors /api/auth/me but more fields)
        [HttpGet]
        [Route("me")]
        public IHttpActionResult GetMe()
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
                        phone = user.Phone,
                        address = user.Address,
                        profession = user.Profession,
                        dateOfBirth = user.DateOfBirth,
                        gender = user.Gender,
                        role = user.Role,
                        isVerified = user.IsVerified,
                        isActive = user.IsActive,
                        createdAt = user.CreatedAt,
                        lastLogin = user.LastLogin
                    }
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/users/me  (update own profile)
        [HttpPut]
        [Route("me")]
        public IHttpActionResult UpdateMe(UpdateProfileRequest request)
        {
            try
            {
                if (request == null) return BadRequest("Invalid request.");

                var userId = JwtHelper.GetUserIdFromToken(Request);
                var user = _context.Users.Find(userId);
                if (user == null) return NotFound();

                // Username is immutable — preventing renames preserves audit trails
                // and avoids conflict with foreign-key references.
                if (!string.IsNullOrWhiteSpace(request.FullName))
                    user.FullName = request.FullName.Trim();
                if (request.Phone != null)
                    user.Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim();
                if (request.Address != null)
                    user.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
                if (request.Profession != null)
                    user.Profession = string.IsNullOrWhiteSpace(request.Profession) ? null : request.Profession.Trim();
                if (request.Gender != null)
                    user.Gender = string.IsNullOrWhiteSpace(request.Gender) ? null : request.Gender.Trim();
                if (request.DateOfBirth.HasValue)
                    user.DateOfBirth = request.DateOfBirth;

                // Optional password change with verification of current password.
                if (!string.IsNullOrWhiteSpace(request.NewPassword))
                {
                    if (string.IsNullOrWhiteSpace(request.CurrentPassword) ||
                        !PasswordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                    {
                        return BadRequest("Current password is incorrect.");
                    }
                    if (request.NewPassword.Length < 6)
                        return BadRequest("New password must be at least 6 characters.");
                    user.PasswordHash = PasswordHasher.Hash(request.NewPassword);
                    // SECURITY: bump PasswordChangedAt so any token issued before
                    // this moment becomes invalid on the next request — forcing
                    // the user (and any session hijacker) to log in again.
                    user.PasswordChangedAt = DateTime.UtcNow;
                }

                user.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Profile updated successfully."
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class UpdateProfileRequest
    {
        [StringLength(150)]
        public string FullName { get; set; }

        [StringLength(20)]
        public string Phone { get; set; }

        [StringLength(255)]
        public string Address { get; set; }

        [StringLength(100)]
        public string Profession { get; set; }

        [StringLength(10)]
        public string Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string CurrentPassword { get; set; }

        [StringLength(100, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}
