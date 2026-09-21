using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Admin endpoints for viewing and retrying the email delivery log.
    /// All actions require SuperAdmin or Admin role.
    /// </summary>
    [RoutePrefix("api/admin/emails")]
    [JwtAuthorize(Roles = "SuperAdmin,Admin")]
    public class AdminEmailLogsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public AdminEmailLogsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/admin/emails
        // Query params: status, category, dateFrom, dateTo, search, page, pageSize
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(
            string status = null,
            string category = null,
            string dateFrom = null,
            string dateTo = null,
            string search = null,
            int page = 1,
            int pageSize = 30)
        {
            try
            {
                var query = _context.EmailLogs.AsQueryable();

                if (!string.IsNullOrWhiteSpace(status))
                    query = query.Where(l => l.Status == status);

                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(l => l.Category == category);

                DateTime? from = null, to = null;
                if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var f))
                    from = f;
                if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var t))
                    to = t.AddDays(1); // inclusive end-of-day

                if (from.HasValue) query = query.Where(l => l.CreatedAt >= from.Value);
                if (to.HasValue)   query = query.Where(l => l.CreatedAt < to.Value);

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var s = search.Trim().ToLower();
                    query = query.Where(l =>
                        l.ToEmail.ToLower().Contains(s) ||
                        l.Subject.ToLower().Contains(s) ||
                        (l.ErrorMessage != null && l.ErrorMessage.ToLower().Contains(s)));
                }

                var total = query.Count();
                var items = query
                    .OrderByDescending(l => l.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(l => new
                    {
                        emailLogId   = l.EmailLogId,
                        toEmail      = l.ToEmail,
                        subject      = l.Subject,
                        category     = l.Category,
                        relatedId    = l.RelatedId,
                        status       = l.Status,
                        sentAt       = l.SentAt,
                        errorMessage = l.ErrorMessage,
                        retryCount   = l.RetryCount,
                        createdAt    = l.CreatedAt
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

        // GET: api/admin/emails/stats
        [HttpGet]
        [Route("stats")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var all = _context.EmailLogs.AsQueryable();
                var today = DateTime.UtcNow.Date;
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        total       = all.Count(),
                        sent        = all.Count(l => l.Status == "Sent"),
                        failed      = all.Count(l => l.Status == "Failed"),
                        mockSent    = all.Count(l => l.Status == "MockSent"),
                        pending     = all.Count(l => l.Status == "Pending"),
                        sentToday   = all.Count(l => l.Status == "Sent" && l.SentAt >= today),
                        failedToday = all.Count(l => l.Status == "Failed" && l.CreatedAt >= today),
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/admin/emails/retry-all
        // Re-attempts all currently-failed rows (up to maxAttempts per row).
        [HttpPost]
        [Route("retry-all")]
        public IHttpActionResult RetryAll([FromBody] RetryRequest request)
        {
            try
            {
                var maxAttempts = request?.MaxAttempts ?? 3;
                if (maxAttempts < 1) maxAttempts = 1;
                if (maxAttempts > 10) maxAttempts = 10;

                var retried = EmailService.RetryFailedEmails(maxAttempts);

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = $"Retry attempted for {retried} email(s). Check the log to confirm delivery."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/admin/emails/{id}/resend
        // Re-sends a single email log entry by re-submitting to SMTP.
        [HttpPost]
        [Route("{id:int}/resend")]
        public IHttpActionResult Resend(int id)
        {
            try
            {
                var log = _context.EmailLogs.Find(id);
                if (log == null) return NotFound();

                // Don't allow unlimited retries from the UI.
                if (log.RetryCount >= 10)
                    return BadRequest($"This email has already been retried {log.RetryCount} times. Manual intervention required.");

                if (!EmailService.SmtpEnabled)
                {
                    // Mock path: just flip status back so it shows in the table as "MockSent".
                    log.Status = "MockSent";
                    log.RetryCount++;
                    _context.SaveChanges();
                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = "Email service is in mock mode. Log entry marked as MockSent."
                    });
                }

                try
                {
                    using (var msg = new System.Net.Mail.MailMessage())
                    {
                        msg.From       = new System.Net.Mail.MailAddress(
                            System.Configuration.ConfigurationManager.AppSettings["SmtpFrom"]
                                ?? "no-reply@care4kids.org",
                            System.Configuration.ConfigurationManager.AppSettings["SmtpFromName"]
                                ?? "Care4Kids");
                        msg.To.Add(log.ToEmail);
                        msg.Subject    = log.Subject;
                        msg.Body       = log.Body;
                        msg.IsBodyHtml = true;

                        using (var client = new System.Net.Mail.SmtpClient(
                            EmailService.SmtpHost,
                            EmailService.SmtpPort))
                        {
                            client.EnableSsl = EmailService.SmtpUseSsl;
                            client.DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network;
                            if (!string.IsNullOrWhiteSpace(EmailService.SmtpUsername))
                            {
                                client.Credentials = new System.Net.NetworkCredential(
                                    EmailService.SmtpUsername,
                                    EmailService.SmtpPassword);
                            }
                            client.Send(msg);
                        }
                    }

                    log.Status    = "Sent";
                    log.SentAt    = DateTime.UtcNow;
                    log.RetryCount++;
                    _context.SaveChanges();

                    return Ok(new ApiResponse
                    {
                        Success = true,
                        Message = $"Email resent successfully to {log.ToEmail}."
                    });
                }
                catch (Exception ex)
                {
                    log.Status      = "Failed";
                    log.ErrorMessage = ex.Message?.Length > 2000
                        ? ex.Message.Substring(0, 2000)
                        : ex.Message;
                    log.RetryCount++;
                    _context.SaveChanges();

                    return Ok(new ApiResponse
                    {
                        Success = false,
                        Message = $"Resend failed: {ex.Message}",
                        Data = new { retryCount = log.RetryCount }
                    });
                }
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class RetryRequest
    {
        public int MaxAttempts { get; set; } = 3;
    }
}
