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
    /// About Us module - Our Supporters page.
    /// Uses the existing Organizations table â€” Supporters, NGO and Partner
    /// types are all displayed, with optional type filter.
    /// </summary>
    [RoutePrefix("api/supporters")]
    public class SupportersController : ApiController
    {
        private readonly GiveAIDContext _context;

        public SupportersController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/supporters
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = true, string type = null)
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
                else
                {
                    // Default: all types that appear on the supporters page
                    query = query.Where(o => o.OrganizationType == "Supporter" ||
                                             o.OrganizationType == "Partner" ||
                                             o.OrganizationType == "NGO");
                }

                var items = query
                    .OrderBy(o => o.DisplayOrder)
                    .ThenBy(o => o.OrganizationName)
                    .ToList()
                    .Select(o => ToDto(o));

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

        // GET: api/supporters/stats
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetStats()
        {
            try
            {
                // PERF: previously 4 separate Count() + 1 Sum = 5 round-trips.
                // Now a single grouped query gives every count + sum in one
                // round-trip.
                var grouped = _context.Organizations
                    .Where(o => o.IsActive &&
                        (o.OrganizationType == "Supporter" ||
                         o.OrganizationType == "Partner" ||
                         o.OrganizationType == "NGO"))
                    .GroupBy(o => o.OrganizationType)
                    .Select(g => new {
                        Type = g.Key,
                        Count = g.Count(),
                        Sum = g.Sum(o => (decimal?)o.ContributionAmount) ?? 0
                    })
                    .ToList();

                int total = grouped.Sum(x => x.Count);
                int supporters = grouped.FirstOrDefault(g => g.Type == "Supporter")?.Count ?? 0;
                int partners = grouped.FirstOrDefault(g => g.Type == "Partner")?.Count ?? 0;
                int ngos = grouped.FirstOrDefault(g => g.Type == "NGO")?.Count ?? 0;
                decimal totalContribution = grouped.Sum(x => x.Sum);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        total,
                        supporters,
                        partners,
                        ngos,
                        totalContribution
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/supporters/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var organization = _context.Organizations.Find(id);
                if (organization == null || !organization.IsActive)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = ToDto(organization, true)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/supporters  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(Organization organization)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                organization.CreatedAt = DateTime.Now;
                organization.UpdatedAt = DateTime.Now;
                organization.IsActive = true;

                _context.Organizations.Add(organization);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Supporter added successfully",
                    Data = ToDto(organization)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/supporters/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, Organization organization)
        {
            try
            {
                var existing = _context.Organizations.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.OrganizationName = organization.OrganizationName;
                existing.OrganizationType = organization.OrganizationType;
                existing.Description = organization.Description;
                existing.LogoUrl = organization.LogoUrl;
                existing.WebsiteUrl = organization.WebsiteUrl;
                existing.ContactEmail = organization.ContactEmail;
                existing.ContactPhone = organization.ContactPhone;
                existing.Address = organization.Address;
                existing.RegistrationNumber = organization.RegistrationNumber;
                existing.Mission = organization.Mission;
                existing.Vision = organization.Vision;
                existing.ContributionAmount = organization.ContributionAmount;
                existing.ContributionType = organization.ContributionType;
                existing.IsActive = organization.IsActive;
                existing.IsFeatured = organization.IsFeatured;
                existing.DisplayOrder = organization.DisplayOrder;
                existing.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Supporter updated successfully",
                    Data = ToDto(existing)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/supporters/5  (SuperAdmin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var organization = _context.Organizations.Find(id);
                if (organization == null)
                {
                    return NotFound();
                }

                // Soft delete to preserve any linked donations
                organization.IsActive = false;
                organization.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Supporter deactivated successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object ToDto(Organization o, bool includeDescription = false)
        {
            return new
            {
                organizationId = o.OrganizationId,
                organizationName = o.OrganizationName,
                organizationType = o.OrganizationType,
                description = includeDescription ? o.Description : null,
                logoUrl = o.LogoUrl,
                websiteUrl = o.WebsiteUrl,
                contactEmail = o.ContactEmail,
                contactPhone = o.ContactPhone,
                address = o.Address,
                mission = o.Mission,
                vision = o.Vision,
                contributionAmount = o.ContributionAmount,
                contributionType = o.ContributionType,
                isActive = o.IsActive,
                isFeatured = o.IsFeatured,
                displayOrder = o.DisplayOrder
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


