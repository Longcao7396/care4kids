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
    /// About Us module - Our Achievements page
    /// Public read endpoints + admin write endpoints.
    /// </summary>
    [RoutePrefix("api/achievements")]
    public class AchievementsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public AchievementsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/achievements
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = true, string category = null)
        {
            try
            {
                var query = _context.Achievements.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(a => a.IsActive);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(a => a.Category == category);
                }

                var items = query
                    .OrderBy(a => a.DisplayOrder)
                    .ThenByDescending(a => a.AchievementDate ?? a.CreatedAt)
                    .ToList()
                    .Select(a => ToDto(a));

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = items
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/achievements/stats
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var query = _context.Achievements.Where(a => a.IsActive);
                var total = query.Count();
                var featured = query.Count(a => a.IsFeatured);
                var totalBeneficiaries = query.Sum(a => (int?)a.Beneficiaries) ?? 0;

                // Highest metric value (for hero card)
                var headline = query
                    .Where(a => a.MetricValue.HasValue)
                    .OrderByDescending(a => a.MetricValue)
                    .FirstOrDefault();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        totalAchievements = total,
                        featuredAchievements = featured,
                        totalBeneficiaries,
                        headline = headline == null ? null : new
                        {
                            headline.Title,
                            headline.MetricValue,
                            headline.MetricLabel,
                            headline.MetricSuffix
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/achievements/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var item = _context.Achievements.Find(id);
                if (item == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = ToDto(item)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/achievements  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(Achievement item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);
                item.CreatedBy = userId;
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;

                _context.Achievements.Add(item);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Achievement created successfully",
                    Data = ToDto(item)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/achievements/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, Achievement item)
        {
            try
            {
                var existing = _context.Achievements.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Title = item.Title;
                existing.Category = item.Category;
                existing.Description = item.Description;
                existing.MetricValue = item.MetricValue;
                existing.MetricLabel = item.MetricLabel;
                existing.MetricSuffix = item.MetricSuffix;
                existing.AchievementDate = item.AchievementDate;
                existing.ImageUrl = item.ImageUrl;
                existing.Icon = item.Icon;
                existing.AwardBy = item.AwardBy;
                existing.Location = item.Location;
                existing.Beneficiaries = item.Beneficiaries;
                existing.DisplayOrder = item.DisplayOrder;
                existing.IsActive = item.IsActive;
                existing.IsFeatured = item.IsFeatured;
                existing.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Achievement updated successfully",
                    Data = ToDto(existing)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/achievements/5  (SuperAdmin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var item = _context.Achievements.Find(id);
                if (item == null)
                {
                    return NotFound();
                }

                _context.Achievements.Remove(item);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Achievement deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object ToDto(Achievement a)
        {
            return new
            {
                achievementId = a.AchievementId,
                title = a.Title,
                category = a.Category,
                description = a.Description,
                metricValue = a.MetricValue,
                metricLabel = a.MetricLabel,
                metricSuffix = a.MetricSuffix,
                achievementDate = a.AchievementDate,
                imageUrl = a.ImageUrl,
                icon = a.Icon,
                awardBy = a.AwardBy,
                location = a.Location,
                beneficiaries = a.Beneficiaries,
                displayOrder = a.DisplayOrder,
                isActive = a.IsActive,
                isFeatured = a.IsFeatured,
                createdAt = a.CreatedAt,
                updatedAt = a.UpdatedAt
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


