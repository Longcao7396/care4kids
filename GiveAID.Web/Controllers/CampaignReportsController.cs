using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Campaign Reports - Public read endpoints + admin write endpoints.
    /// Reports are published impact/transparency reports attached to a Campaign,
    /// showing how donated funds were spent and what was achieved.
    /// </summary>
    [RoutePrefix("api/campaign-reports")]
    public class CampaignReportsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public CampaignReportsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/campaign-reports
        // Public: returns only published reports by default.
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            bool publishedOnly = true,
            int? campaignId = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 20;
                if (pageSize > 100) pageSize = 100;

                var query = _context.CampaignReports.AsQueryable();

                if (publishedOnly)
                {
                    query = query.Where(r => r.IsPublished);
                }

                if (campaignId.HasValue)
                {
                    query = query.Where(r => r.CampaignId == campaignId.Value);
                }

                var total = query.Count();
                var items = query
                    .OrderByDescending(r => r.PublishedDate ?? r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(r => ToDto(r))
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
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

        // GET: api/campaign-reports/campaign/5
        // All reports for a specific campaign (public: published only).
        [HttpGet]
        [Route("campaign/{campaignId:int}")]
        public IHttpActionResult GetByCampaign(int campaignId, bool publishedOnly = true)
        {
            try
            {
                var query = _context.CampaignReports
                    .Where(r => r.CampaignId == campaignId);

                if (publishedOnly)
                {
                    query = query.Where(r => r.IsPublished);
                }

                var items = query
                    .OrderByDescending(r => r.PublishedDate ?? r.CreatedAt)
                    .ToList()
                    .Select(r => ToDto(r))
                    .ToList();

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

        // GET: api/campaign-reports/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var item = _context.CampaignReports.Find(id);
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

        // GET: api/campaign-reports/stats
        // Aggregate stats for admin dashboard / campaign reports page.
        [HttpGet]
        [Route("stats")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var stats = _context.CampaignReports
                    .GroupBy(r => 1)
                    .Select(g => new
                    {
                        Total = g.Count(),
                        Published = g.Count(r => r.IsPublished),
                        Drafts = g.Count(r => !r.IsPublished),
                        TotalReceived = g.Sum(r => (decimal?)r.TotalReceived) ?? 0m,
                        TotalSpent = g.Sum(r => (decimal?)r.TotalSpent) ?? 0m,
                        Beneficiaries = g.Sum(r => (int?)r.BeneficiariesReached) ?? 0
                    })
                    .FirstOrDefault();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        totalReports = stats?.Total ?? 0,
                        publishedReports = stats?.Published ?? 0,
                        draftReports = stats?.Drafts ?? 0,
                        totalReceived = stats?.TotalReceived ?? 0m,
                        totalSpent = stats?.TotalSpent ?? 0m,
                        remainingAmount = (stats?.TotalReceived ?? 0m) - (stats?.TotalSpent ?? 0m),
                        totalBeneficiaries = stats?.Beneficiaries ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/campaign-reports  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(CampaignReport item)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate FK
                var campaign = _context.Campaigns.Find(item.CampaignId);
                if (campaign == null)
                {
                    return BadRequest("Campaign not found.");
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);
                item.CreatedAt = DateTime.Now;
                item.UpdatedAt = DateTime.Now;

                if (item.IsPublished && !item.PublishedDate.HasValue)
                {
                    item.PublishedDate = DateTime.Now;
                    item.PublishedBy = userId;
                }

                _context.CampaignReports.Add(item);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Campaign report created successfully",
                    Data = ToDto(item)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/campaign-reports/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, CampaignReport item)
        {
            try
            {
                var existing = _context.CampaignReports.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.CampaignId = item.CampaignId;
                existing.TotalReceived = item.TotalReceived;
                existing.TotalSpent = item.TotalSpent;
                existing.BeneficiariesReached = item.BeneficiariesReached;
                existing.ReportTitle = item.ReportTitle;
                existing.ReportContent = item.ReportContent;
                existing.ExpenseBreakdown = item.ExpenseBreakdown;
                existing.Photos = item.Photos;
                existing.Documents = item.Documents;

                // Publishing transition: was false -> now true => stamp PublishedDate/By
                var wasPublished = existing.IsPublished;
                existing.IsPublished = item.IsPublished;

                if (!wasPublished && item.IsPublished && !existing.PublishedDate.HasValue)
                {
                    existing.PublishedDate = DateTime.Now;
                    existing.PublishedBy = JwtHelper.GetUserIdFromToken(Request);
                }

                existing.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Campaign report updated successfully",
                    Data = ToDto(existing)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/campaign-reports/5  (SuperAdmin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var item = _context.CampaignReports.Find(id);
                if (item == null)
                {
                    return NotFound();
                }

                _context.CampaignReports.Remove(item);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Campaign report deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object ToDto(CampaignReport r)
        {
            return new
            {
                reportId = r.ReportId,
                campaignId = r.CampaignId,
                totalReceived = r.TotalReceived,
                totalSpent = r.TotalSpent,
                remainingAmount = r.RemainingAmount,
                beneficiariesReached = r.BeneficiariesReached,
                reportTitle = r.ReportTitle,
                reportContent = r.ReportContent,
                expenseBreakdown = r.ExpenseBreakdown,
                photos = r.Photos,
                documents = r.Documents,
                isPublished = r.IsPublished,
                publishedDate = r.PublishedDate,
                publishedBy = r.PublishedBy,
                createdAt = r.CreatedAt,
                updatedAt = r.UpdatedAt
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
