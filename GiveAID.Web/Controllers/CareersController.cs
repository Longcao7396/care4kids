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
    /// About Us module - Career page
    /// Public list + apply, admin CRUD on jobs, admin read on applications.
    /// </summary>
    [RoutePrefix("api/careers")]
    public class CareersController : ApiController
    {
        private readonly GiveAIDContext _context;

        public CareersController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/careers
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = true, string department = null, string employmentType = null)
        {
            try
            {
                var query = _context.Careers.AsQueryable();

                if (activeOnly)
                {
                    var today = DateTime.Today;
                    query = query.Where(c => c.IsActive &&
                        (c.ClosingDate == null || c.ClosingDate >= today));
                }

                if (!string.IsNullOrEmpty(department))
                {
                    query = query.Where(c => c.Department == department);
                }

                if (!string.IsNullOrEmpty(employmentType))
                {
                    query = query.Where(c => c.EmploymentType == employmentType);
                }

                var items = query
                    .OrderByDescending(c => c.PostedDate)
                    .ToList()
                    .Select(c => ToJobDto(c));

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

        // GET: api/careers/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var career = _context.Careers.Find(id);
                if (career == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = ToJobDto(career, true)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/careers/5/apply  (public â€” anyone can apply)
        [HttpPost]
        [Route("{id:int}/apply")]
        public IHttpActionResult Apply(int id, CareerApplyRequest request)
        {
            try
            {
                // SECURITY: rate-limit career applications per IP. 3 per 5 minutes
                // prevents drive-by application floods.
                if (RateLimiter.IsLimited("careers-apply", maxRequests: 3, windowSeconds: 300))
                {
                        return Content((System.Net.HttpStatusCode)429,
                        new ApiResponse
                        {
                            Success = false,
                            Message = "Too many applications submitted from your IP. Please try again later."
                        });
                }

                if (request == null ||
                    string.IsNullOrWhiteSpace(request.ApplicantName) ||
                    string.IsNullOrWhiteSpace(request.Email))
                {
                    return BadRequest("Applicant name and email are required.");
                }

                var career = _context.Careers.Find(id);
                if (career == null)
                {
                    return NotFound();
                }

                if (!career.IsActive || (career.ClosingDate.HasValue && career.ClosingDate.Value < DateTime.Today))
                {
                    return BadRequest("This position is no longer accepting applications.");
                }

                var application = new CareerApplication
                {
                    CareerId = id,
                    ApplicantName = request.ApplicantName,
                    Email = request.Email,
                    Phone = request.Phone,
                    ResumeUrl = request.ResumeUrl,
                    CoverLetter = request.CoverLetter,
                    LinkedInUrl = request.LinkedInUrl,
                    PortfolioUrl = request.PortfolioUrl,
                    Status = "Submitted",
                    AppliedAt = DateTime.Now
                };

                _context.CareerApplications.Add(application);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Application submitted successfully",
                    Data = new
                    {
                        applicationId = application.ApplicationId,
                        careerId = application.CareerId,
                        positionTitle = career.PositionTitle,
                        appliedAt = application.AppliedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/careers/5/applications  (Admin only)
        [HttpGet]
        [Route("{id:int}/applications")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetApplications(int id)
        {
            try
            {
                var apps = _context.CareerApplications
                    .Where(a => a.CareerId == id)
                    .OrderByDescending(a => a.AppliedAt)
                    .ToList()
                    .Select(a => new
                    {
                        applicationId = a.ApplicationId,
                        careerId = a.CareerId,
                        applicantName = a.ApplicantName,
                        email = a.Email,
                        phone = a.Phone,
                        resumeUrl = a.ResumeUrl,
                        coverLetter = a.CoverLetter,
                        linkedInUrl = a.LinkedInUrl,
                        portfolioUrl = a.PortfolioUrl,
                        status = a.Status,
                        appliedAt = a.AppliedAt
                    });

                return Ok(new ApiResponse { Success = true, Data = apps });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/careers  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(Career career)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);
                career.CreatedBy = userId;
                career.CreatedAt = DateTime.Now;
                career.PostedDate = career.PostedDate == default ? DateTime.Today : career.PostedDate;

                _context.Careers.Add(career);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Career posted successfully",
                    Data = ToJobDto(career)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/careers/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, Career career)
        {
            try
            {
                var existing = _context.Careers.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.PositionTitle = career.PositionTitle;
                existing.Department = career.Department;
                existing.Description = career.Description;
                existing.Requirements = career.Requirements;
                existing.Responsibilities = career.Responsibilities;
                existing.Location = career.Location;
                existing.EmploymentType = career.EmploymentType;
                existing.SalaryRange = career.SalaryRange;
                existing.Vacancies = career.Vacancies <= 0 ? 1 : career.Vacancies;
                existing.PostedDate = career.PostedDate == default ? existing.PostedDate : career.PostedDate;
                existing.ClosingDate = career.ClosingDate;
                existing.IsActive = career.IsActive;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Career updated successfully",
                    Data = ToJobDto(existing)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/careers/5  (SuperAdmin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var career = _context.Careers.Find(id);
                if (career == null)
                {
                    return NotFound();
                }

                // Soft delete to preserve foreign key from applications
                career.IsActive = false;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Career deactivated successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object ToJobDto(Career c, bool includeDescription = true)
        {
            return new
            {
                careerId = c.CareerId,
                positionTitle = c.PositionTitle,
                department = c.Department,
                description = includeDescription ? c.Description : null,
                requirements = c.Requirements,
                responsibilities = c.Responsibilities,
                location = c.Location,
                employmentType = c.EmploymentType,
                salaryRange = c.SalaryRange,
                vacancies = c.Vacancies,
                postedDate = c.PostedDate,
                closingDate = c.ClosingDate,
                isActive = c.IsActive,
                createdAt = c.CreatedAt
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

    public class CareerApplyRequest
    {
        public string ApplicantName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ResumeUrl { get; set; }
        public string CoverLetter { get; set; }
        public string LinkedInUrl { get; set; }
        public string PortfolioUrl { get; set; }
    }
}


