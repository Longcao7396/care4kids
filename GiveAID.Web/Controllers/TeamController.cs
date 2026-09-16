using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// About Us module - Our Team page
    /// Public read endpoints + admin write endpoints.
    /// </summary>
    [RoutePrefix("api/team")]
    public class TeamController : ApiController
    {
        private readonly GiveAIDContext _context;

        public TeamController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/team
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            bool activeOnly = true,
            bool featuredOnly = false,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var query = _context.TeamMembers.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(m => m.IsActive);
                }

                if (featuredOnly)
                {
                    query = query.Where(m => m.IsFeatured);
                }

                var total = query.Count();
                var members = query
                    .OrderBy(m => m.DisplayOrder)
                    .ThenBy(m => m.FullName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(m => ToDto(m))
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        items = members,
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

        // GET: api/team/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var member = _context.TeamMembers.Find(id);
                if (member == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = ToDto(member)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/team  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(TeamMember member)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);
                member.CreatedBy = userId;
                member.CreatedAt = DateTime.Now;
                member.UpdatedAt = DateTime.Now;

                _context.TeamMembers.Add(member);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Team member created successfully",
                    Data = ToDto(member)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/team/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, TeamMember member)
        {
            try
            {
                var existing = _context.TeamMembers.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.FullName = member.FullName;
                existing.RoleTitle = member.RoleTitle;
                existing.Department = member.Department;
                existing.Bio = member.Bio;
                existing.PhotoUrl = member.PhotoUrl;
                existing.Email = member.Email;
                existing.LinkedInUrl = member.LinkedInUrl;
                existing.TwitterUrl = member.TwitterUrl;
                existing.FacebookUrl = member.FacebookUrl;
                existing.DisplayOrder = member.DisplayOrder;
                existing.IsActive = member.IsActive;
                existing.IsFeatured = member.IsFeatured;
                existing.JoinedDate = member.JoinedDate;
                existing.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Team member updated successfully",
                    Data = ToDto(existing)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/team/5  (SuperAdmin only - hard delete)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var member = _context.TeamMembers.Find(id);
                if (member == null)
                {
                    return NotFound();
                }

                _context.TeamMembers.Remove(member);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Team member deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object ToDto(TeamMember m)
        {
            return new
            {
                teamMemberId = m.TeamMemberId,
                fullName = m.FullName,
                roleTitle = m.RoleTitle,
                department = m.Department,
                bio = m.Bio,
                photoUrl = m.PhotoUrl,
                email = m.Email,
                linkedInUrl = m.LinkedInUrl,
                twitterUrl = m.TwitterUrl,
                facebookUrl = m.FacebookUrl,
                displayOrder = m.DisplayOrder,
                isActive = m.IsActive,
                isFeatured = m.IsFeatured,
                joinedDate = m.JoinedDate,
                createdAt = m.CreatedAt,
                updatedAt = m.UpdatedAt
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}


