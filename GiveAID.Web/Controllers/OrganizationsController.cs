using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Organizations â€” full CRUD for all organization types:
    /// NGO, Partner, Supporter, Corporate, Government, Other.
    /// Uses the existing Organizations table.
    /// </summary>
    [RoutePrefix("api/organizations")]
    public class OrganizationsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public OrganizationsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/organizations
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            bool activeOnly = false,
            string type = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.Organizations.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(o => o.IsActive);
                }

                if (!string.IsNullOrEmpty(type))
                {
                    query = query.Where(o => o.OrganizationType == type);
                }

                var total = query.Count();

                var items = query
                    .OrderBy(o => o.DisplayOrder)
                    .ThenBy(o => o.OrganizationName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(o => ToDto(o));

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        items,
                        pagination = new
                        {
                            total,
                            page,
                            pageSize,
                            totalPages = (int)Math.Ceiling((double)total / pageSize)
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/organizations/stats
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetStats()
        {
            try
            {
                // PERF: previously 6 separate Count() calls (1 + 1 + 5 byType + 1 Sum)
                // — each was a separate round-trip to SQL Server. Now a single
                // grouped query gives us every count + the active sum in one
                // round-trip.
                var byTypeCounts = _context.Organizations
                    .Where(o => o.IsActive)
                    .GroupBy(o => o.OrganizationType)
                    .Select(g => new { Type = g.Key, Count = g.Count() })
                    .ToList();
                var byType = byTypeCounts.ToDictionary(x => x.Type ?? "Other", x => x.Count);

                var totalActive = byTypeCounts.Sum(x => x.Count);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        total = totalActive,
                        active = totalActive,
                        byType = new
                        {
                            NGO = byType.TryGetValue("NGO", out var c1) ? c1 : 0,
                            Partner = byType.TryGetValue("Partner", out var c2) ? c2 : 0,
                            Supporter = byType.TryGetValue("Supporter", out var c3) ? c3 : 0,
                            Corporate = byType.TryGetValue("Corporate", out var c4) ? c4 : 0,
                            Government = byType.TryGetValue("Government", out var c5) ? c5 : 0,
                            Other = byType.TryGetValue("Other", out var c6) ? c6 : 0,
                        },
                        totalContribution = _context.Organizations
                            .Where(o => o.IsActive)
                            .Sum(o => (decimal?)o.ContributionAmount) ?? 0
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/organizations/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var o = _context.Organizations.Find(id);
                if (o == null) return NotFound();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = ToDto(o, includeDetails: true)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/organizations  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(OrganizationRequest request)
        {
            try
            {
                var errs = ValidateRequest(request);
                if (errs.Length > 0) return BadRequest(string.Join("; ", errs));

                var org = new Organization
                {
                    OrganizationName = request.OrganizationName.Trim(),
                    OrganizationType = request.OrganizationType ?? "NGO",
                    Description = request.Description?.Trim(),
                    LogoUrl = request.LogoUrl?.Trim(),
                    WebsiteUrl = request.WebsiteUrl?.Trim(),
                    ContactEmail = request.ContactEmail?.Trim(),
                    ContactPhone = request.ContactPhone?.Trim(),
                    Address = request.Address?.Trim(),
                    RegistrationNumber = request.RegistrationNumber?.Trim(),
                    Mission = request.Mission?.Trim(),
                    Vision = request.Vision?.Trim(),
                    ContributionAmount = request.ContributionAmount,
                    ContributionType = request.ContributionType?.Trim(),
                    IsActive = true,
                    IsFeatured = request.IsFeatured,
                    DisplayOrder = request.DisplayOrder,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Organizations.Add(org);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Organization created successfully.",
                    Data = new { organizationId = org.OrganizationId, name = org.OrganizationName }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/organizations/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, OrganizationRequest request)
        {
            try
            {
                var org = _context.Organizations.Find(id);
                if (org == null) return NotFound();

                var errs = ValidateRequest(request, isUpdate: true);
                if (errs.Length > 0) return BadRequest(string.Join("; ", errs));

                org.OrganizationName = request.OrganizationName.Trim();
                org.OrganizationType = request.OrganizationType ?? org.OrganizationType;
                org.Description = request.Description?.Trim();
                org.LogoUrl = request.LogoUrl?.Trim();
                org.WebsiteUrl = request.WebsiteUrl?.Trim();
                org.ContactEmail = request.ContactEmail?.Trim();
                org.ContactPhone = request.ContactPhone?.Trim();
                org.Address = request.Address?.Trim();
                org.RegistrationNumber = request.RegistrationNumber?.Trim();
                org.Mission = request.Mission?.Trim();
                org.Vision = request.Vision?.Trim();
                org.ContributionAmount = request.ContributionAmount;
                org.ContributionType = request.ContributionType?.Trim();
                org.IsActive = request.IsActive;
                org.IsFeatured = request.IsFeatured;
                org.DisplayOrder = request.DisplayOrder;
                org.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Organization updated successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/organizations/5  (SuperAdmin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var org = _context.Organizations.Find(id);
                if (org == null) return NotFound();

                // Soft delete
                org.IsActive = false;
                org.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Organization deactivated successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static string[] ValidateRequest(OrganizationRequest req, bool isUpdate = false)
        {
            var errs = new System.Collections.Generic.List<string>();
            if (!isUpdate && string.IsNullOrWhiteSpace(req.OrganizationName))
                errs.Add("Organization name is required.");
            if (!isUpdate && string.IsNullOrWhiteSpace(req.OrganizationType))
                errs.Add("Organization type is required.");
            if (!string.IsNullOrWhiteSpace(req.ContactEmail) &&
                !System.Text.RegularExpressions.Regex.IsMatch(req.ContactEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                errs.Add("Invalid email address.");
            if (!string.IsNullOrWhiteSpace(req.WebsiteUrl) &&
                !System.Text.RegularExpressions.Regex.IsMatch(req.WebsiteUrl, @"^https?://.+"))
                errs.Add("Website must start with http:// or https://");
            return errs.ToArray();
        }

        private static object ToDto(Organization o, bool includeDetails = false)
        {
            var dto = new
            {
                organizationId = o.OrganizationId,
                organizationName = o.OrganizationName,
                organizationType = o.OrganizationType,
                description = o.Description,
                logoUrl = o.LogoUrl,
                websiteUrl = o.WebsiteUrl,
                contactEmail = o.ContactEmail,
                contactPhone = o.ContactPhone,
                address = o.Address,
                contributionAmount = o.ContributionAmount,
                contributionType = o.ContributionType,
                isFeatured = o.IsFeatured,
                displayOrder = o.DisplayOrder,
                isActive = o.IsActive,
                createdAt = o.CreatedAt,
                updatedAt = o.UpdatedAt
            };

            if (includeDetails)
            {
                return new
                {
                    dto.organizationId,
                    dto.organizationName,
                    dto.organizationType,
                    dto.description,
                    dto.logoUrl,
                    dto.websiteUrl,
                    dto.contactEmail,
                    dto.contactPhone,
                    dto.address,
                    dto.contributionAmount,
                    dto.contributionType,
                    registrationNumber = o.RegistrationNumber,
                    mission = o.Mission,
                    vision = o.Vision,
                    dto.isFeatured,
                    dto.displayOrder,
                    dto.isActive,
                    dto.createdAt,
                    dto.updatedAt
                };
            }

            return dto;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class OrganizationRequest
    {
        public string OrganizationName { get; set; }
        public string OrganizationType { get; set; }
        public string Description { get; set; }
        public string LogoUrl { get; set; }
        public string WebsiteUrl { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public string Address { get; set; }
        public string RegistrationNumber { get; set; }
        public string Mission { get; set; }
        public string Vision { get; set; }
        public decimal? ContributionAmount { get; set; }
        public string ContributionType { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsFeatured { get; set; }
        public int DisplayOrder { get; set; }
    }
}


