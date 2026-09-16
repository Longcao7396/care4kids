using System;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;
using GiveAID.Web.Controllers;

namespace GiveAID.Web.Controllers
{
    [RoutePrefix("api/causes")]
    public class CausesController : ApiController
    {
        private readonly GiveAIDContext _context;

        public CausesController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/causes?activeOnly=true&parentsOnly=true
        // GET: api/causes/5
        // GET: api/causes/5/sub-causes
        // GET: api/causes/stats
        // GET: api/causes/tree

        // GET: api/causes — list causes. Supports filters:
        //   activeOnly    — filter by IsActive
        //   parentsOnly   — only top-level causes (no parent_cause_id)
        //   subOf={id}    — only direct sub-causes of the given parent
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = true, bool parentsOnly = false, int? subOf = null)
        {
            try
            {
                var query = _context.Causes.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(c => c.IsActive);
                }

                if (parentsOnly)
                {
                    query = query.Where(c => c.ParentCauseId == null);
                }

                if (subOf.HasValue)
                {
                    query = query.Where(c => c.ParentCauseId == subOf.Value);
                }

                var causes = query.OrderBy(c => c.DisplayOrder).ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = causes.Select(MapToDto).ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/causes/tree — full hierarchy (parents with embedded sub-causes).
        // Convenient for the public CausesPage where one render pass shows the
        // whole taxonomy. By default filters active parents and active sub-items.
        [HttpGet]
        [Route("tree")]
        public IHttpActionResult GetTree(bool activeOnly = true)
        {
            try
            {
                var query = _context.Causes.AsQueryable();
                if (activeOnly)
                {
                    query = query.Where(c => c.IsActive);
                }

                var parents = query
                    .Where(c => c.ParentCauseId == null)
                    .OrderBy(c => c.DisplayOrder)
                    .ToList();

                var allSubs = query
                    .Where(c => c.ParentCauseId != null)
                    .OrderBy(c => c.DisplayOrder)
                    .ToList();

                var tree = parents.Select(p => new
                {
                    parent = MapToDto(p),
                    subCauses = allSubs
                        .Where(s => s.ParentCauseId == p.CauseId)
                        .Select(MapToDto)
                        .ToList()
                }).ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = tree
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/causes/5/sub-causes
        [HttpGet]
        [Route("{id:int}/sub-causes")]
        public IHttpActionResult GetSubCauses(int id, bool activeOnly = true)
        {
            try
            {
                var parent = _context.Causes.Find(id);
                if (parent == null)
                {
                    return NotFound();
                }

                var query = _context.Causes.Where(c => c.ParentCauseId == id);
                if (activeOnly)
                {
                    query = query.Where(c => c.IsActive);
                }

                var subs = query.OrderBy(c => c.DisplayOrder).Select(MapToDto).ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = subs
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/causes/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var cause = _context.Causes.Find(id);

                if (cause == null)
                {
                    return NotFound();
                }

                // Also pull sub-causes so the admin/detail view can show them inline.
                var subCauses = _context.Causes
                    .Where(c => c.ParentCauseId == id)
                    .OrderBy(c => c.DisplayOrder)
                    .Select(MapToDto)
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        cause = MapToDto(cause),
                        subCauses
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /// <summary>
        /// Shared mapper — anonymous type so we can evolve the wire format
        /// without breaking entity consumers.
        /// </summary>
        private static object MapToDto(Cause c)
        {
            return new
            {
                causeId = c.CauseId,
                causeCode = c.CauseCode,
                causeName = c.CauseName,
                description = c.Description,
                imageUrl = c.ImageUrl,
                icon = c.Icon,
                targetAmount = c.TargetAmount,
                raisedAmount = c.RaisedAmount,
                percentageReached = c.PercentageReached,
                isActive = c.IsActive,
                displayOrder = c.DisplayOrder,
                parentCauseId = c.ParentCauseId,
                isParentCause = c.ParentCauseId == null,
                createdAt = c.CreatedAt
            };
        }

        // GET: api/causes/stats
        [HttpGet]
        [Route("stats")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var totalCauses = _context.Causes.Count();
                var activeCauses = _context.Causes.Count(c => c.IsActive);
                var totalRaised = _context.Causes.Sum(c => (decimal?)c.RaisedAmount) ?? 0;
                var totalTarget = _context.Causes.Sum(c => (decimal?)c.TargetAmount) ?? 0;

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        totalCauses,
                        activeCauses,
                        totalRaised,
                        totalTarget,
                        overallPercentage = totalTarget > 0 ? (totalRaised / totalTarget) * 100 : 0
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/causes
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(Cause cause)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                cause.CreatedAt = DateTime.Now;
                _context.Causes.Add(cause);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Cause created successfully",
                    Data = cause
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/causes/5
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, Cause cause)
        {
            try
            {
                var existing = _context.Causes.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.CauseName = cause.CauseName;
                existing.Description = cause.Description;
                existing.ImageUrl = cause.ImageUrl;
                existing.Icon = cause.Icon;
                existing.TargetAmount = cause.TargetAmount;
                existing.IsActive = cause.IsActive;
                existing.DisplayOrder = cause.DisplayOrder;
                existing.ParentCauseId = cause.ParentCauseId;
                existing.UpdatedAt = DateTime.Now;

                if (!string.IsNullOrWhiteSpace(cause.CauseCode))
                {
                    existing.CauseCode = cause.CauseCode;
                }

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Cause updated successfully",
                    Data = existing
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/causes/5
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var cause = _context.Causes.Find(id);
                if (cause == null)
                {
                    return NotFound();
                }

                // Soft delete
                cause.IsActive = false;
                cause.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Cause deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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


