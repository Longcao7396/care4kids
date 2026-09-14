using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    [RoutePrefix("api/admin/users")]
    public class AdminUsersController : ApiController
    {
        private GiveAIDContext db = new GiveAIDContext();

        private bool IsAdmin(GiveAIDContext ctx, out User actor)
        {
            actor = null;
            var userId = JwtHelper.GetUserIdFromToken(Request);
            if (userId == 0) return false;
            actor = ctx.Users.Find(userId);
            return actor != null && (actor.Role == "Admin" || actor.Role == "SuperAdmin");
        }

        // GET: api/admin/users
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            int page = 1,
            int pageSize = 20,
            string role = null,
            string search = null)
        {
            try
            {
                if (!IsAdmin(db, out _))
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Admin access required" });

                var query = db.Users.AsQueryable();

                if (!string.IsNullOrWhiteSpace(role))
                    query = query.Where(u => u.Role == role);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(u =>
                        u.FullName.ToLower().Contains(s) ||
                        u.Email.ToLower().Contains(s) ||
                        u.Username.ToLower().Contains(s));
                }

                var total = query.Count();
                var items = query
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.UserId,
                        u.Username,
                        u.Email,
                        u.FullName,
                        u.Phone,
                        u.Role,
                        u.IsActive,
                        u.IsVerified,
                        u.CreatedAt,
                        u.LastLogin,
                        donationCount = db.Donations.Count(d => d.UserId == u.UserId && d.PaymentStatus == "Completed"),
                        totalDonated = db.Donations
                            .Where(d => d.UserId == u.UserId && d.PaymentStatus == "Completed")
                            .Sum(d => (decimal?)d.Amount) ?? 0
                    })
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        items,
                        total,
                        page,
                        pageSize,
                        totalPages = (int)Math.Ceiling((double)total / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/admin/users/{id}/role
        [HttpPut]
        [Route("{id:int}/role")]
        public IHttpActionResult UpdateRole(int id, [FromBody] RoleUpdateDto payload)
        {
            try
            {
                if (!IsAdmin(db, out var actor))
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Admin access required" });

                var target = db.Users.Find(id);
                if (target == null) return NotFound();

                var newRole = payload?.Role;
                if (string.IsNullOrWhiteSpace(newRole))
                    return BadRequest("Role is required.");

                // Only SuperAdmin can promote to Admin/SuperAdmin or demote an Admin
                if ((newRole == "Admin" || newRole == "SuperAdmin") && actor.Role != "SuperAdmin")
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Only SuperAdmin can assign Admin or SuperAdmin roles." });

                if (target.Role == "SuperAdmin" && actor.Role != "SuperAdmin")
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Only a SuperAdmin can change another SuperAdmin." });

                target.Role = newRole;
                target.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Ok(new { success = true, message = "Role updated.", data = new { target.UserId, target.Role } });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/admin/users/{id}/status
        [HttpPut]
        [Route("{id:int}/status")]
        public IHttpActionResult UpdateStatus(int id, [FromBody] StatusUpdateDto payload)
        {
            try
            {
                if (!IsAdmin(db, out var actor))
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Admin access required" });

                var target = db.Users.Find(id);
                if (target == null) return NotFound();

                if (target.Role == "SuperAdmin" && actor.Role != "SuperAdmin")
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Only a SuperAdmin can change another SuperAdmin." });

                target.IsActive = payload?.IsActive ?? target.IsActive;
                target.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Ok(new { success = true, message = "Status updated.", data = new { target.UserId, target.IsActive } });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/admin/users/{id} (soft delete by deactivating)
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                if (!IsAdmin(db, out var actor))
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Admin access required" });

                var target = db.Users.Find(id);
                if (target == null) return NotFound();

                if (target.Role == "SuperAdmin")
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Cannot deactivate a SuperAdmin." });

                // Soft delete: deactivate instead of removing
                target.IsActive = false;
                target.UpdatedAt = DateTime.Now;
                db.SaveChanges();

                return Ok(new { success = true, message = "User deactivated." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        public class RoleUpdateDto
        {
            public string Role { get; set; }
        }

        public class StatusUpdateDto
        {
            public bool? IsActive { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) db.Dispose();
            base.Dispose(disposing);
        }
    }
}
