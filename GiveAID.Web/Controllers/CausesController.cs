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

        // GET: api/causes
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = true)
        {
            try
            {
                var query = _context.Causes.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(c => c.IsActive);
                }

                var causes = query.OrderBy(c => c.DisplayOrder).ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = causes.Select(c => new
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
                        createdAt = c.CreatedAt
                    }).ToList()
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

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        causeId = cause.CauseId,
                        causeCode = cause.CauseCode,
                        causeName = cause.CauseName,
                        description = cause.Description,
                        imageUrl = cause.ImageUrl,
                        icon = cause.Icon,
                        targetAmount = cause.TargetAmount,
                        raisedAmount = cause.RaisedAmount,
                        percentageReached = cause.PercentageReached,
                        isActive = cause.IsActive,
                        displayOrder = cause.DisplayOrder,
                        createdAt = cause.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
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
                existing.UpdatedAt = DateTime.Now;

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


