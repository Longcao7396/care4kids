using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// User Invite Friends â€” records referral invitations and queues them
    /// for sending. The actual email send is delegated to <see cref="EmailService"/>
    /// which currently logs the message (mock) â€” once a real SMTP / provider
    /// is configured, only that helper needs to change.
    /// </summary>
    [RoutePrefix("api/invitations")]
    [JwtAuthorize]
    public class InvitationsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public InvitationsController()
        {
            _context = new GiveAIDContext();
        }

        // POST: api/invitations  (any authenticated user)
        [HttpPost]
        [Route("")]
        public IHttpActionResult Send(InvitationRequest request)
        {
            try
            {
                var errs = Validate(request);
                if (errs.Length > 0) return BadRequest(string.Join("; ", errs));

                var userId = JwtHelper.GetUserIdFromToken(Request);
                var inviter = _context.Users.Find(userId);
                if (inviter == null) return Unauthorized();

                // Don't allow inviting yourself
                if (string.Equals(inviter.Email, request.InviteeEmail, StringComparison.OrdinalIgnoreCase))
                    return BadRequest("You cannot invite yourself.");

                // Optional: cap how many invites a user can send per day (anti-abuse)
                var today = DateTime.Today;
                var sentToday = _context.Invitations.Count(i =>
                    i.InviterUserId == userId && i.CreatedAt >= today);
                if (sentToday >= 20)
                    return BadRequest("Daily invitation limit reached. Please try again tomorrow.");

                // Optional: skip if there's already a pending invite to the same email
                var alreadyPending = _context.Invitations.Any(i =>
                    i.InviterUserId == userId &&
                    i.InviteeEmail == request.InviteeEmail &&
                    (i.Status == "Pending" || i.Status == "Sent"));
                if (alreadyPending)
                    return BadRequest("You already have an active invitation for this email.");

                var token = GenerateToken();

                var inv = new Invitation
                {
                    InviterUserId = userId,
                    InviteeName = request.InviteeName.Trim(),
                    InviteeEmail = request.InviteeEmail.Trim(),
                    PersonalMessage = request.PersonalMessage?.Trim(),
                    Status = "Pending",
                    InvitationToken = token,
                    CreatedAt = DateTime.Now
                };

                _context.Invitations.Add(inv);
                _context.SaveChanges();

                // Try to dispatch via the (currently mock) email service.
                var dispatch = EmailService.SendInvitation(inv, inviter);
                if (dispatch.Success)
                {
                    inv.Status = "Sent";
                    inv.SentAt = DateTime.Now;
                }
                else
                {
                    inv.Status = "Failed";
                    inv.FailureReason = dispatch.Error;
                }
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = dispatch.Success
                        ? $"Invitation queued for {request.InviteeEmail}."
                        : "Invitation recorded, but the email service is not currently configured. The recipient has been added to the invite list.",
                    Data = new
                    {
                        invitationId = inv.InvitationId,
                        status = inv.Status,
                        inviteeEmail = inv.InviteeEmail,
                        sentAt = inv.SentAt,
                        emailServiceActive = dispatch.Success
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/invitations/mine â€” current user's sent invitations
        [HttpGet]
        [Route("mine")]
        public IHttpActionResult GetMine()
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);
                var items = _context.Invitations
                    .Where(i => i.InviterUserId == userId)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        invitationId = i.InvitationId,
                        inviteeName = i.InviteeName,
                        inviteeEmail = i.InviteeEmail,
                        personalMessage = i.PersonalMessage,
                        status = i.Status,
                        sentAt = i.SentAt,
                        registeredAt = i.RegisteredAt,
                        failureReason = i.FailureReason,
                        createdAt = i.CreatedAt
                    })
                    .ToList();

                return Ok(new ApiResponse { Success = true, Data = items });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/invitations/stats â€” admin view
        [HttpGet]
        [Route("stats")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var all = _context.Invitations.AsQueryable();
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        total = all.Count(),
                        pending = all.Count(i => i.Status == "Pending"),
                        sent = all.Count(i => i.Status == "Sent"),
                        failed = all.Count(i => i.Status == "Failed"),
                        registered = all.Count(i => i.Status == "Registered"),
                        cancelled = all.Count(i => i.Status == "Cancelled"),
                        last30Days = all.Count(i => i.CreatedAt >= DateTime.Now.AddDays(-30))
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/invitations â€” admin list
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetAll(string status = null, int page = 1, int pageSize = 30)
        {
            try
            {
                var query = _context.Invitations
                    .Include("Inviter")
                    .AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(i => i.Status == status);

                var total = query.Count();
                var items = query
                    .OrderByDescending(i => i.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(i => new
                    {
                        invitationId = i.InvitationId,
                        inviterUserId = i.InviterUserId,
                        inviterName = i.Inviter != null ? i.Inviter.FullName : "â€”",
                        inviteeName = i.InviteeName,
                        inviteeEmail = i.InviteeEmail,
                        personalMessage = i.PersonalMessage,
                        status = i.Status,
                        sentAt = i.SentAt,
                        registeredAt = i.RegisteredAt,
                        failureReason = i.FailureReason,
                        createdAt = i.CreatedAt
                    })
                    .ToList();

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

        // POST: api/invitations/{id}/cancel â€” admin only
        [HttpPost]
        [Route("{id:int}/cancel")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Cancel(int id)
        {
            try
            {
                var inv = _context.Invitations.Find(id);
                if (inv == null) return NotFound();
                if (inv.Status == "Registered") return BadRequest("Cannot cancel a registered invitation.");

                inv.Status = "Cancelled";
                inv.UpdatedAtSafe();
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Invitation cancelled."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/invitations/accept/{token}  â€” public, called when recipient signs up
        [HttpPost]
        [Route("accept/{token}")]
        [AllowAnonymous]
        public IHttpActionResult AcceptByToken(string token)
        {
            try
            {
                var inv = _context.Invitations.FirstOrDefault(i => i.InvitationToken == token);
                if (inv == null) return NotFound();
                if (inv.Status == "Registered") return Ok(new ApiResponse { Success = true, Message = "Already accepted." });
                if (inv.Status == "Cancelled") return BadRequest("Invitation cancelled.");

                inv.Status = "Registered";
                inv.RegisteredAt = DateTime.Now;
                inv.UpdatedAtSafe();
                _context.SaveChanges();

                return Ok(new ApiResponse { Success = true, Message = "Invitation accepted." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /* â”€â”€ helpers â”€â”€ */
        private static string[] Validate(InvitationRequest req)
        {
            var errs = new System.Collections.Generic.List<string>();
            if (string.IsNullOrWhiteSpace(req.InviteeName))
                errs.Add("Friend's name is required.");
            if (string.IsNullOrWhiteSpace(req.InviteeEmail))
                errs.Add("Friend's email is required.");
            else if (!System.Text.RegularExpressions.Regex.IsMatch(
                req.InviteeEmail, @"^[^\s@]+@[^\s@]+\.[^\s@]+$"))
                errs.Add("Please enter a valid email address.");
            if (!string.IsNullOrEmpty(req.PersonalMessage) && req.PersonalMessage.Length > 500)
                errs.Add("Personal message must be 500 characters or less.");
            return errs.ToArray();
        }

        private static string GenerateToken()
        {
            return Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N").Substring(0, 8);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class InvitationRequest
    {
        public string InviteeName { get; set; }
        public string InviteeEmail { get; set; }
        public string PersonalMessage { get; set; }
    }
}

