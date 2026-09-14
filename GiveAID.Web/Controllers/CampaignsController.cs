using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;

namespace GiveAID.Web.Controllers
{
    [RoutePrefix("api/campaigns")]
    public class CampaignsController : ApiController
    {
        private GiveAIDContext db = new GiveAIDContext();

        // GET: api/campaigns
        // Unified list â€” supports donation campaigns AND merged programme-style events.
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            string status = null,
            int? causeId = null,
            bool? featured = null,
            string search = null,
            // Optional: filter to events (RegistrationRequired = true) only.
            bool eventsOnly = false,
            int page = 1,
            int pageSize = 50)
        {
            try
            {
                // Fetch all active causes into a lookup dict to avoid EF lazy-loading
                // issues (EF6 tries to materialize Cause nav property before
                // MapCampaign runs, which can throw if any FK is broken).
                var causeLookup = db.Causes
                    .Where(ca => ca.IsActive)
                    .ToDictionary(ca => ca.CauseId, ca => ca);

                var query = db.Campaigns.AsQueryable();

                if (!string.IsNullOrEmpty(status) && status.ToLower() != "all")
                {
                    query = query.Where(c => c.Status == status);
                }

                if (causeId.HasValue)
                {
                    query = query.Where(c => c.CauseId == causeId.Value);
                }

                if (featured.HasValue)
                {
                    query = query.Where(c => c.IsFeatured == featured.Value);
                }

                if (eventsOnly)
                {
                    query = query.Where(c => c.RegistrationRequired);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(c =>
                        c.CampaignName.ToLower().Contains(term) ||
                        (c.Description != null && c.Description.ToLower().Contains(term)) ||
                        (c.Location != null && c.Location.ToLower().Contains(term)) ||
                        (c.ProgrammeType != null && c.ProgrammeType.ToLower().Contains(term))
                    );
                }

                query = query.OrderByDescending(c => c.IsFeatured)
                             .ThenBy(c => c.DisplayOrder)
                             .ThenByDescending(c => c.CreatedAt);

                var total = query.Count();

                var campaigns = query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(c => MapCampaign(c, causeLookup))
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = campaigns,
                    pagination = new
                    {
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

        // GET: api/campaigns/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var causeLookup = db.Causes
                    .Where(ca => ca.IsActive)
                    .ToDictionary(ca => ca.CauseId, ca => ca);

                var campaign = db.Campaigns
                    .FirstOrDefault(c => c.CampaignId == id);

                if (campaign == null)
                {
                    return NotFound();
                }

                var donorCount = db.Donations
                    .Where(d => d.CampaignId == id && d.PaymentStatus == "Completed")
                    .Select(d => d.UserId)
                    .Distinct()
                    .Count();

                var recentDonations = db.Donations
                    .Where(d => d.CampaignId == id && d.PaymentStatus == "Completed" && !d.IsAnonymous)
                    .OrderByDescending(d => d.DonationDate)
                    .Take(10)
                    .Select(d => new { d.UserId, d.Amount, d.Message, d.DonationDate })
                    .ToList()
                    .Select(d => {
                        var user = db.Users.Find(d.UserId);
                        return new CampaignDonationItem {
                            fullName = user?.FullName ?? "Anonymous",
                            amount = d.Amount,
                            message = d.Message,
                            donationDate = d.DonationDate
                        };
                    })
                    .ToList();

                var donationBreakdown = db.Donations
                    .Where(d => d.CampaignId == id && d.PaymentStatus == "Completed")
                    .GroupBy(d => d.Amount >= 5000000 ? "5,000,000+ VNÄ" :
                                  d.Amount >= 1000000 ? "1,000,000 - 4,999,999 VNÄ" :
                                  d.Amount >= 500000 ? "500,000 - 999,999 VNÄ" :
                                  d.Amount >= 100000 ? "100,000 - 499,999 VNÄ" : "< 100,000 VNÄ")
                    .Select(g => new CampaignDonationBreakdown
                    {
                        range = g.Key,
                        count = g.Count(),
                        total = g.Sum(d => d.Amount)
                    })
                    .ToList();

                var currentParticipants = campaign.RegistrationRequired
                    ? db.CampaignRegistrations.Count(r =>
                        r.CampaignId == id && r.Status != "Cancelled")
                    : (int?)null;

                var data = MapCampaign(campaign, causeLookup);
                // Augment with detail-only fields.
                data.recentDonationCount = donorCount;
                data.recentDonations = recentDonations;
                data.donationBreakdown = donationBreakdown;

                return Ok(new { success = true, data = data });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/campaigns (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(CampaignCreateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);

                var cause = db.Causes.Find(request.CauseId);
                if (cause == null) return BadRequest("Invalid cause");

                var campaign = new Campaign
                {
                    CauseId = request.CauseId,
                    OrganizationId = request.OrganizationId,
                    CampaignName = request.CampaignName,
                    CampaignCode = request.CampaignCode,
                    Description = request.Description,
                    GoalAmount = request.GoalAmount,
                    RaisedAmount = 0,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    ImageUrl = request.ImageUrl,
                    BeneficiariesCount = request.BeneficiariesCount,
                    TargetBeneficiaries = request.TargetBeneficiaries,
                    Location = request.Location,
                    Status = string.IsNullOrEmpty(request.Status) ? "Active" : request.Status,
                    IsFeatured = request.IsFeatured ?? false,
                    DisplayOrder = request.DisplayOrder ?? 0,

                    // Merged programme fields
                    ProgrammeType = request.ProgrammeType,
                    RegistrationRequired = request.RegistrationRequired ?? false,
                    MaxParticipants = request.MaxParticipants,
                    ExpectedBudget = request.ExpectedBudget,
                    ActualBudget = request.ActualBudget,

                    CreatedBy = userId,
                    CreatedAt = DateTime.Now
                };

                db.Campaigns.Add(campaign);
                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Campaign created successfully",
                    data = new
                    {
                        campaignId = campaign.CampaignId,
                        campaignName = campaign.CampaignName,
                        campaignCode = campaign.CampaignCode
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/campaigns/5 (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, CampaignUpdateRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var campaign = db.Campaigns.Find(id);
                if (campaign == null)
                {
                    return NotFound();
                }

                if (request.CauseId.HasValue)
                {
                    var cause = db.Causes.Find(request.CauseId.Value);
                    if (cause == null) return BadRequest("Invalid cause.");
                    campaign.CauseId = request.CauseId.Value;
                }
                if (request.OrganizationId.HasValue)
                {
                    campaign.OrganizationId = request.OrganizationId.Value;
                }
                campaign.CampaignName = request.CampaignName ?? campaign.CampaignName;
                campaign.Description = request.Description ?? campaign.Description;
                campaign.GoalAmount = request.GoalAmount ?? campaign.GoalAmount;
                campaign.StartDate = request.StartDate ?? campaign.StartDate;
                campaign.EndDate = request.EndDate ?? campaign.EndDate;
                campaign.ImageUrl = request.ImageUrl ?? campaign.ImageUrl;
                campaign.BeneficiariesCount = request.BeneficiariesCount ?? campaign.BeneficiariesCount;
                campaign.TargetBeneficiaries = request.TargetBeneficiaries ?? campaign.TargetBeneficiaries;
                campaign.Location = request.Location ?? campaign.Location;
                campaign.Status = request.Status ?? campaign.Status;
                campaign.IsFeatured = request.IsFeatured ?? campaign.IsFeatured;
                campaign.DisplayOrder = request.DisplayOrder ?? campaign.DisplayOrder;

                // Programme fields
                campaign.ProgrammeType = request.ProgrammeType ?? campaign.ProgrammeType;
                if (request.RegistrationRequired.HasValue)
                {
                    campaign.RegistrationRequired = request.RegistrationRequired.Value;
                }
                campaign.MaxParticipants = request.MaxParticipants ?? campaign.MaxParticipants;
                campaign.ExpectedBudget = request.ExpectedBudget ?? campaign.ExpectedBudget;
                campaign.ActualBudget = request.ActualBudget ?? campaign.ActualBudget;

                campaign.UpdatedAt = DateTime.Now;

                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Campaign updated successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/campaigns/5 (Admin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var campaign = db.Campaigns.Find(id);
                if (campaign == null)
                {
                    return NotFound();
                }

                var hasDonations = db.Donations.Any(d => d.CampaignId == id);
                if (hasDonations)
                {
                    return BadRequest("Cannot delete campaign with existing donations. Consider marking it as Cancelled instead.");
                }

                db.Campaigns.Remove(campaign);
                db.SaveChanges();

                return Ok(new
                {
                    success = true,
                    message = "Campaign deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/campaigns/featured
        [HttpGet]
        [Route("featured")]
        public IHttpActionResult GetFeatured(int count = 3)
        {
            try
            {
                var causeLookup = db.Causes
                    .Where(ca => ca.IsActive)
                    .ToDictionary(ca => ca.CauseId, ca => ca);

                var campaigns = db.Campaigns
                    .Where(c => c.IsFeatured && c.Status == "Active")
                    .OrderBy(c => c.DisplayOrder)
                    .Take(count)
                    .ToList()
                    .Select(c => MapCampaign(c, causeLookup))
                    .ToList();

                return Ok(new { success = true, data = campaigns });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/campaigns/{id}/register
        // Replaces POST /api/programmes/{id}/register.
        [HttpPost]
        [Route("{id:int}/register")]
        [JwtAuthorize]
        public IHttpActionResult Register(int id, CampaignRegistrationRequest request)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);

                using (var transaction = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    var campaign = db.Campaigns.Find(id);
                    if (campaign == null)
                    {
                        return NotFound();
                    }

                    if (!campaign.RegistrationRequired)
                    {
                        return BadRequest("This campaign does not require registration");
                    }

                    // Accept registration in any non-cancelled status
                    if (campaign.Status == "Cancelled")
                    {
                        return BadRequest("Registration is closed for this campaign");
                    }

                    if (db.CampaignRegistrations.Any(r =>
                        r.CampaignId == id && r.UserId == userId))
                    {
                        return BadRequest("You are already registered for this campaign");
                    }

                    var currentParticipants = db.CampaignRegistrations.Count(r =>
                        r.CampaignId == id && r.Status != "Cancelled");
                    if (campaign.MaxParticipants.HasValue &&
                        currentParticipants >= campaign.MaxParticipants.Value)
                    {
                        return BadRequest("This campaign is full");
                    }

                    var registration = new CampaignRegistration
                    {
                        CampaignId = id,
                        UserId = userId,
                        Status = "Registered",
                        Notes = request?.Notes,
                        AttendanceConfirmed = false,
                        RegistrationDate = DateTime.Now
                    };

                    db.CampaignRegistrations.Add(registration);
                    db.SaveChanges();
                    transaction.Commit();

                    return Ok(new
                    {
                        success = true,
                        message = "Successfully registered for the campaign",
                        data = new
                        {
                            registrationId = registration.RegistrationId,
                            campaignId = campaign.CampaignId,
                            campaignName = campaign.CampaignName,
                            registrationDate = registration.RegistrationDate
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/campaigns/my-registrations
        // Replaces GET /api/programmes/my-registrations.
        [HttpGet]
        [Route("my-registrations")]
        [JwtAuthorize]
        public IHttpActionResult GetMyRegistrations()
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);

                var registrations = db.CampaignRegistrations
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RegistrationDate)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = registrations.Select(r => {
                        var campaign = db.Campaigns.Find(r.CampaignId);
                        return new
                        {
                            registrationId = r.RegistrationId,
                            campaignId = r.CampaignId,
                            campaignName = campaign?.CampaignName ?? "(Deleted)",
                            programmeType = campaign?.ProgrammeType,
                            campaignStatus = campaign?.Status,
                            startDate = campaign?.StartDate,
                            endDate = campaign?.EndDate,
                            location = campaign?.Location,
                            notes = r.Notes,
                            attendanceConfirmed = r.AttendanceConfirmed,
                            registrationDate = r.RegistrationDate
                        };
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/campaigns/{id}/registrations (Admin)
        [HttpGet]
        [Route("{id:int}/registrations")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetRegistrations(int id)
        {
            try
            {
                var registrations = db.CampaignRegistrations
                    .Where(r => r.CampaignId == id)
                    .OrderByDescending(r => r.RegistrationDate)
                    .ToList();

                return Ok(new
                {
                    success = true,
                    data = registrations.Select(r => {
                        var user = db.Users.Find(r.UserId);
                        return new
                        {
                            registrationId = r.RegistrationId,
                            userId = r.UserId,
                            userName = user?.FullName ?? "(Deleted)",
                            userEmail = user?.Email ?? "",
                            notes = r.Notes,
                            attendanceConfirmed = r.AttendanceConfirmed,
                            registrationDate = r.RegistrationDate
                        };
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // â”€â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

        // Returns a unified shape that the frontend can render for both
        // donation campaigns and event-style (registration-based) campaigns.
        private CampaignDto MapCampaign(Campaign c, Dictionary<int, Cause> causeLookup)
        {
            int? currentParticipants = null;
            if (c.RegistrationRequired)
            {
                currentParticipants = db.CampaignRegistrations.Count(r =>
                    r.CampaignId == c.CampaignId && r.Status != "Cancelled");
            }

            return new CampaignDto
            {
                campaignId = c.CampaignId,
                causeId = c.CauseId,
                cause = !causeLookup.TryGetValue(c.CauseId, out var cause) ? null : new CampaignCauseDto
                {
                    causeId = cause.CauseId,
                    causeName = cause.CauseName,
                    causeCode = cause.CauseCode,
                    icon = cause.Icon
                },
                campaignName = c.CampaignName,
                campaignCode = c.CampaignCode,
                description = c.Description,
                goalAmount = c.GoalAmount,
                raisedAmount = c.RaisedAmount,
                percentageReached = c.PercentageReached,
                startDate = c.StartDate,
                endDate = c.EndDate,
                daysRemaining = c.DaysRemaining,
                imageUrl = c.ImageUrl,
                beneficiariesCount = c.BeneficiariesCount,
                location = c.Location,
                status = c.Status,
                isFeatured = c.IsFeatured,
                displayOrder = c.DisplayOrder,
                donorCount = db.Donations
                    .Where(d => d.CampaignId == c.CampaignId && d.PaymentStatus == "Completed")
                    .Select(d => d.UserId)
                    .Distinct()
                    .Count(),
                createdAt = c.CreatedAt,

                // Merged Programme fields (always present, even if null/false)
                programmeType = c.ProgrammeType,
                registrationRequired = c.RegistrationRequired,
                maxParticipants = c.MaxParticipants,
                currentParticipants = currentParticipants,
                targetBeneficiaries = c.TargetBeneficiaries,
                expectedBudget = c.ExpectedBudget,
                actualBudget = c.ActualBudget,
                organizationId = c.OrganizationId,
                organizationName = c.OrganizationId.HasValue
                    ? db.Organizations.Where(o => o.OrganizationId == c.OrganizationId.Value)
                        .Select(o => o.OrganizationName).FirstOrDefault()
                    : null,

                // Convenience discriminator for frontend â€” 'donation' vs 'event'
                campaignKind = c.RegistrationRequired ? "event" : "donation"
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class CampaignRegistrationRequest
    {
        public string Notes { get; set; }
    }

    public class CampaignCreateRequest
    {
        public int CauseId { get; set; }
        public int? OrganizationId { get; set; }
        public string CampaignName { get; set; }
        public string CampaignCode { get; set; }
        public string Description { get; set; }
        public decimal GoalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ImageUrl { get; set; }
        public int? BeneficiariesCount { get; set; }
        public int? TargetBeneficiaries { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public bool? IsFeatured { get; set; }
        public int? DisplayOrder { get; set; }

        // Merged programme fields
        public string ProgrammeType { get; set; }
        public bool? RegistrationRequired { get; set; }
        public int? MaxParticipants { get; set; }
        public decimal? ExpectedBudget { get; set; }
        public decimal? ActualBudget { get; set; }
    }

    public class CampaignUpdateRequest
    {
        public int? CauseId { get; set; }
        public int? OrganizationId { get; set; }
        public string CampaignName { get; set; }
        public string CampaignCode { get; set; }
        public string Description { get; set; }
        public decimal? GoalAmount { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ImageUrl { get; set; }
        public int? BeneficiariesCount { get; set; }
        public int? TargetBeneficiaries { get; set; }
        public string Location { get; set; }
        public string Status { get; set; }
        public bool? IsFeatured { get; set; }
        public int? DisplayOrder { get; set; }

        // Merged programme fields
        public string ProgrammeType { get; set; }
        public bool? RegistrationRequired { get; set; }
        public int? MaxParticipants { get; set; }
        public decimal? ExpectedBudget { get; set; }
        public decimal? ActualBudget { get; set; }
    }
}



