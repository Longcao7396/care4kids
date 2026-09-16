using System;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web.Http;
using System.Data.Entity;
using GiveAID.Web.Data;
using GiveAID.Web.Models;
using GiveAID.Web.Helpers;
using GiveAID.Web.Controllers;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// LEGACY: ProgrammesController is kept for backward compatibility with
    /// existing data in the Programmes table. New work should target the
    /// Campaign entity (CampaignsController), which has absorbed the programme
    /// fields (programmeType, registrationRequired, maxParticipants, etc.) via
    /// the CampaignProgramme_Merge.sql migration.
    ///
    /// All write endpoints require Admin/SuperAdmin role via [JwtAuthorize].
    /// </summary>
    [RoutePrefix("api/programmes")]
    [Obsolete("Use CampaignsController. Programmes data has been merged into Campaigns.")]
    public class ProgrammesController : ApiController
    {
        private readonly GiveAIDContext _context;

        public ProgrammesController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/programmes
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(string programmeType = null, string status = null, int page = 1, int pageSize = 10, bool featuredOnly = false)
        {
            try
            {
                var query = _context.Programmes
                    .Include(p => p.Organization)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(programmeType))
                {
                    query = query.Where(p => p.ProgrammeType == programmeType);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(p => p.Status == status);
                }

                if (featuredOnly)
                {
                    query = query.Where(p => p.IsFeatured &&
                        (p.Status == "Upcoming" || p.Status == "Ongoing"));
                }

                var total = query.Count();
                var programmes = query
                    .OrderByDescending(p => p.StartDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        items = programmes.Select(p => new
                        {
                            programmeId = p.ProgrammeId,
                            organizationId = p.OrganizationId,
                            organizationName = p.Organization?.OrganizationName,
                            title = p.Title,
                            programmeType = p.ProgrammeType,
                            description = p.Description,
                            imageUrl = p.ImageUrl,
                            startDate = p.StartDate,
                            endDate = p.EndDate,
                            location = p.Location,
                            targetBeneficiaries = p.TargetBeneficiaries,
                            status = p.Status,
                            registrationRequired = p.RegistrationRequired,
                            maxParticipants = p.MaxParticipants,
                            currentParticipants = _context.ProgrammeRegistrations.Count(r =>
                                r.ProgrammeId == p.ProgrammeId && r.Status != "Cancelled"),
                            createdAt = p.CreatedAt
                        }).ToList(),
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

        // GET: api/programmes/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var programme = _context.Programmes
                    .Include(p => p.Organization)
                    .FirstOrDefault(p => p.ProgrammeId == id);

                if (programme == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        programmeId = programme.ProgrammeId,
                        organizationId = programme.OrganizationId,
                        organizationName = programme.Organization?.OrganizationName,
                        title = programme.Title,
                        programmeType = programme.ProgrammeType,
                        description = programme.Description,
                        imageUrl = programme.ImageUrl,
                        startDate = programme.StartDate,
                        endDate = programme.EndDate,
                        location = programme.Location,
                        targetBeneficiaries = programme.TargetBeneficiaries,
                        expectedBudget = programme.ExpectedBudget,
                        actualBudget = programme.ActualBudget,
                        status = programme.Status,
                        registrationRequired = programme.RegistrationRequired,
                        maxParticipants = programme.MaxParticipants,
                        currentParticipants = _context.ProgrammeRegistrations.Count(r =>
                            r.ProgrammeId == programme.ProgrammeId && r.Status != "Cancelled"),
                        createdAt = programme.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/programmes/{id}/register
        [HttpPost]
        [Route("{id:int}/register")]
        [JwtAuthorize]
        public IHttpActionResult Register(int id, ProgrammeRegistrationRequest request)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);

                // PERF: previously this used IsolationLevel.Serializable around the
                // capacity-check + insert, which held a wide range lock and was a
                // bottleneck under concurrent registrations. Replaced with:
                //   1. Compute current count from a regular query.
                //   2. Rely on the unique index (ProgrammeId, UserId) at the DB
                //      level to prevent duplicate registrations — catch
                //      DbUpdateException to convert it into a clean 400.
                // The capacity check is best-effort: in a true race two users can
                // both pass the check and both insert, briefly exceeding capacity.
                // For an NGO this is acceptable; for stricter semantics add an
                // explicit UPDLOCK on the count query.
                var programme = _context.Programmes.Find(id);
                if (programme == null)
                {
                    return NotFound();
                }

                if (!programme.RegistrationRequired)
                {
                    return BadRequest("This programme does not require registration");
                }

                if (programme.Status != "Upcoming")
                {
                    return BadRequest("Registration is closed for this programme");
                }

                if (_context.ProgrammeRegistrations.Any(r =>
                    r.ProgrammeId == id && r.UserId == userId))
                {
                    return BadRequest("You are already registered for this programme");
                }

                var currentParticipants = _context.ProgrammeRegistrations.Count(r =>
                    r.ProgrammeId == id && r.Status != "Cancelled");
                if (programme.MaxParticipants.HasValue &&
                    currentParticipants >= programme.MaxParticipants.Value)
                {
                    return BadRequest("This programme is full");
                }

                var registration = new ProgrammeRegistration
                {
                    ProgrammeId = id,
                    UserId = userId,
                    Status = "Registered",
                    Notes = request.MotivationMessage?.Trim(),
                    AttendanceConfirmed = false,
                    RegistrationDate = DateTime.Now
                };

                try
                {
                    _context.ProgrammeRegistrations.Add(registration);
                    _context.SaveChanges();
                }
                catch (DbUpdateException)
                {
                    // Unique-index violation = concurrent duplicate insert. Treat
                    // it as a duplicate-registration error.
                    return BadRequest("You are already registered for this programme");
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Successfully registered for the programme",
                    Data = new
                    {
                        registrationId = registration.RegistrationId,
                        programmeTitle = programme.Title,
                        registrationDate = registration.RegistrationDate
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/programmes/my-registrations
        [HttpGet]
        [Route("my-registrations")]
        [JwtAuthorize]
        public IHttpActionResult GetMyRegistrations()
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);

                var registrations = _context.ProgrammeRegistrations
                    .Include(r => r.Programme)
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.RegistrationDate)
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = registrations.Select(r => new
                    {
                        registrationId = r.RegistrationId,
                        programmeId = r.ProgrammeId,
                        programmeTitle = r.Programme.Title,
                        programmeType = r.Programme.ProgrammeType,
                        programmeStatus = r.Programme.Status,
                        programmeStartDate = r.Programme.StartDate,
                        location = r.Programme.Location,
                        motivationMessage = r.Notes,
                        attendanceConfirmed = r.AttendanceConfirmed,
                        registrationDate = r.RegistrationDate
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/programmes/{id}/registrations
        [HttpGet]
        [Route("{id:int}/registrations")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetRegistrations(int id)
        {
            try
            {
                var registrations = _context.ProgrammeRegistrations
                    .Include(r => r.User)
                    .Where(r => r.ProgrammeId == id)
                    .OrderByDescending(r => r.RegistrationDate)
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = registrations.Select(r => new
                    {
                        registrationId = r.RegistrationId,
                        userId = r.UserId,
                        userName = r.User.FullName,
                        userEmail = r.User.Email,
                        motivationMessage = r.Notes,
                        attendanceConfirmed = r.AttendanceConfirmed,
                        registrationDate = r.RegistrationDate
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/programmes
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(Programme programme)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                programme.CreatedAt = DateTime.Now;
                programme.UpdatedAt = DateTime.Now;
                _context.Programmes.Add(programme);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Programme created successfully",
                    Data = programme
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/programmes/5
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, Programme programme)
        {
            try
            {
                var existing = _context.Programmes.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Title = programme.Title;
                existing.ProgrammeType = programme.ProgrammeType;
                existing.Description = programme.Description;
                existing.ImageUrl = programme.ImageUrl;
                existing.StartDate = programme.StartDate;
                existing.EndDate = programme.EndDate;
                existing.Location = programme.Location;
                existing.TargetBeneficiaries = programme.TargetBeneficiaries;
                existing.ExpectedBudget = programme.ExpectedBudget;
                existing.ActualBudget = programme.ActualBudget;
                existing.Status = programme.Status;
                existing.IsFeatured = programme.IsFeatured;
                existing.RegistrationRequired = programme.RegistrationRequired;
                existing.MaxParticipants = programme.MaxParticipants;
                existing.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Programme updated successfully",
                    Data = existing
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

    public class ProgrammeRegistrationRequest
    {
        public string MotivationMessage { get; set; }
    }
}

