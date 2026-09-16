using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// User-to-Admin conversations — also used for "Raise Query" so users can
    /// create a question and view its status, while admins reply in-thread.
    /// Uses the existing Conversations + ConversationMessages tables.
    /// </summary>
    [RoutePrefix("api/conversations")]
    public class ConversationsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public ConversationsController()
        {
            _context = new GiveAIDContext();
        }

        // POST: api/conversations  (any authenticated user) — raises a new query
        [HttpPost]
        [Route("")]
        [JwtAuthorize]
        public IHttpActionResult Create(ConversationCreateRequest request)
        {
            try
            {
                var errs = ValidateCreate(request);
                if (errs.Length > 0) return BadRequest(string.Join("; ", errs));

                var userId = JwtHelper.GetUserIdFromToken(Request);
                var user = _context.Users.Find(userId);
                if (user == null) return Unauthorized();

                var conv = new Conversation
                {
                    UserId = userId,
                    Subject = request.Subject.Trim(),
                    ConversationType = request.ConversationType?.Trim() ?? "Query",
                    Status = "Open",
                    Priority = string.IsNullOrEmpty(request.Priority) ? "Normal" : request.Priority,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _context.Conversations.Add(conv);
                _context.SaveChanges();

                // First message — the user's question
                var firstMsg = new ConversationMessage
                {
                    ConversationId = conv.ConversationId,
                    SenderId = userId,
                    MessageText = request.Message.Trim(),
                    IsInternalNote = false,
                    CreatedAt = DateTime.Now
                };
                _context.ConversationMessages.Add(firstMsg);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Your query has been submitted. Our team will reply shortly.",
                    Data = new
                    {
                        conversationId = conv.ConversationId,
                        status = conv.Status,
                        subject = conv.Subject,
                        priority = conv.Priority,
                        createdAt = conv.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/conversations/{id}/messages  — user or admin replies
        [HttpPost]
        [Route("{id:int}/messages")]
        [JwtAuthorize]
        public IHttpActionResult AddMessage(int id, MessageRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request?.MessageText))
                    return BadRequest("Message text is required.");

                var conv = _context.Conversations.Find(id);
                if (conv == null) return NotFound();

                var userId = JwtHelper.GetUserIdFromToken(Request);
                var user = _context.Users.Find(userId);
                if (user == null) return Unauthorized();

                // Authorization: only the original raiser or an admin can reply.
                bool isAdmin = user.Role == "Admin" || user.Role == "SuperAdmin";
                if (!isAdmin && conv.UserId != userId)
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "You can only reply to your own queries." });

                var msg = new ConversationMessage
                {
                    ConversationId = id,
                    SenderId = userId,
                    MessageText = request.MessageText.Trim(),
                    IsInternalNote = false,
                    CreatedAt = DateTime.Now
                };
                _context.ConversationMessages.Add(msg);

                // Auto-flip status when an admin replies
                if (isAdmin)
                {
                    if (conv.Status == "Open") conv.Status = "InProgress";
                    conv.AssignedTo = conv.AssignedTo ?? userId;
                }
                else
                {
                    // user replied — keep status but bump updated_at
                }

                conv.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Message added.",
                    Data = new
                    {
                        messageId = msg.MessageId,
                        conversationId = id,
                        createdAt = msg.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/conversations/{id}/close — admin or original user
        [HttpPost]
        [Route("{id:int}/close")]
        [JwtAuthorize]
        public IHttpActionResult Close(int id)
        {
            try
            {
                var conv = _context.Conversations.Find(id);
                if (conv == null) return NotFound();

                var userId = JwtHelper.GetUserIdFromToken(Request);
                var user = _context.Users.Find(userId);
                if (user == null) return Unauthorized();

                bool isAdmin = user.Role == "Admin" || user.Role == "SuperAdmin";
                if (!isAdmin && conv.UserId != userId)
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "You can only close your own queries." });

                conv.Status = "Closed";
                conv.ClosedAt = DateTime.Now;
                conv.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new ApiResponse { Success = true, Message = "Conversation closed." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/conversations/mine — current user's queries
        [HttpGet]
        [Route("mine")]
        [JwtAuthorize]
        public IHttpActionResult GetMine(string status = null)
        {
            try
            {
                var userId = JwtHelper.GetUserIdFromToken(Request);
                var query = _context.Conversations.Where(c => c.UserId == userId);
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.Status == status);

                var items = query
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToList()
                    .Select(c => new
                    {
                        conversationId = c.ConversationId,
                        subject = c.Subject,
                        conversationType = c.ConversationType,
                        status = c.Status,
                        priority = c.Priority,
                        messageCount = _context.ConversationMessages.Count(m => m.ConversationId == c.ConversationId),
                        lastMessageAt = c.UpdatedAt,
                        createdAt = c.CreatedAt,
                        closedAt = c.ClosedAt
                    })
                    .ToList();

                return Ok(new ApiResponse { Success = true, Data = new { items, total = items.Count } });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/conversations/{id} — detail incl. messages
        [HttpGet]
        [Route("{id:int}")]
        [JwtAuthorize]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var conv = _context.Conversations.Find(id);
                if (conv == null) return NotFound();

                var userId = JwtHelper.GetUserIdFromToken(Request);
                var user = _context.Users.Find(userId);
                if (user == null) return Unauthorized();

                bool isAdmin = user.Role == "Admin" || user.Role == "SuperAdmin";
                if (!isAdmin && conv.UserId != userId)
                    return Content(System.Net.HttpStatusCode.Forbidden,
                        new { success = false, message = "Not authorized to view this conversation." });

                var messages = _context.ConversationMessages
                    .Where(m => m.ConversationId == id && !m.IsInternalNote)
                    .OrderBy(m => m.CreatedAt)
                    .ToList()
                    .Select(m => new
                    {
                        messageId = m.MessageId,
                        senderId = m.SenderId,
                        senderName = ResolveSenderName(m.SenderId),
                        isFromAdmin = IsAdmin(m.SenderId),
                        messageText = m.MessageText,
                        createdAt = m.CreatedAt
                    })
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        conversationId = conv.ConversationId,
                        userId = conv.UserId,
                        userName = ResolveSenderName(conv.UserId ?? 0),
                        subject = conv.Subject,
                        conversationType = conv.ConversationType,
                        status = conv.Status,
                        priority = conv.Priority,
                        assignedTo = conv.AssignedTo,
                        createdAt = conv.CreatedAt,
                        updatedAt = conv.UpdatedAt,
                        closedAt = conv.ClosedAt,
                        messages
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/conversations — admin list
        [HttpGet]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetAll(string status = null, string priority = null,
            string type = null, int page = 1, int pageSize = 20)
        {
            try
            {
                var query = _context.Conversations.AsQueryable();

                if (!string.IsNullOrEmpty(status))
                    query = query.Where(c => c.Status == status);
                if (!string.IsNullOrEmpty(priority))
                    query = query.Where(c => c.Priority == priority);
                if (!string.IsNullOrEmpty(type))
                    query = query.Where(c => c.ConversationType == type);

                var total = query.Count();
                var items = query
                    .OrderByDescending(c => c.UpdatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(c => new
                    {
                        conversationId = c.ConversationId,
                        userId = c.UserId,
                        userName = c.UserId.HasValue ? ResolveSenderName(c.UserId.Value) : "—",
                        subject = c.Subject,
                        conversationType = c.ConversationType,
                        status = c.Status,
                        priority = c.Priority,
                        assignedTo = c.AssignedTo,
                        assignedToName = c.AssignedTo.HasValue ? ResolveSenderName(c.AssignedTo.Value) : null,
                        messageCount = _context.ConversationMessages.Count(m => m.ConversationId == c.ConversationId),
                        createdAt = c.CreatedAt,
                        updatedAt = c.UpdatedAt,
                        closedAt = c.ClosedAt
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

        // GET: api/conversations/stats — admin
        [HttpGet]
        [Route("stats")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var all = _context.Conversations.AsQueryable();
                var since = DateTime.Now.AddDays(-7); // compute outside LINQ
                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        total = all.Count(),
                        open = all.Count(c => c.Status == "Open"),
                        inProgress = all.Count(c => c.Status == "InProgress"),
                        closed = all.Count(c => c.Status == "Closed"),
                        highPriority = all.Count(c => c.Priority == "High" && c.Status != "Closed"),
                        last7Days = all.Count(c => c.CreatedAt >= since)
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/conversations/{id}/assign — admin only
        [HttpPost]
        [Route("{id:int}/assign")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Assign(int id, AssignRequest request)
        {
            try
            {
                var conv = _context.Conversations.Find(id);
                if (conv == null) return NotFound();

                conv.AssignedTo = request.AssignedTo;
                conv.UpdatedAt = DateTime.Now;
                _context.SaveChanges();

                return Ok(new ApiResponse { Success = true, Message = "Assigned." });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        /* ── helpers ── */
        private static string[] ValidateCreate(ConversationCreateRequest req)
        {
            var errs = new System.Collections.Generic.List<string>();
            if (req == null)
            {
                errs.Add("Request body is required.");
                return errs.ToArray();
            }
            if (string.IsNullOrWhiteSpace(req.Subject))
                errs.Add("Subject is required.");
            if (string.IsNullOrWhiteSpace(req.Message))
                errs.Add("Please describe your question / message.");
            if (!string.IsNullOrEmpty(req.Priority) &&
                req.Priority != "Low" && req.Priority != "Normal" && req.Priority != "High")
                errs.Add("Invalid priority.");
            return errs.ToArray();
        }

        private string ResolveSenderName(int userId)
        {
            var u = _context.Users.Find(userId);
            return u != null ? u.FullName : "Unknown";
        }

        private bool IsAdmin(int userId)
        {
            var u = _context.Users.Find(userId);
            return u != null && (u.Role == "Admin" || u.Role == "SuperAdmin");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing) _context.Dispose();
            base.Dispose(disposing);
        }
    }

    public class ConversationCreateRequest
    {
        public string Subject { get; set; }
        public string ConversationType { get; set; }
        public string Message { get; set; }
        public string Priority { get; set; }
    }

    public class MessageRequest
    {
        public string MessageText { get; set; }
    }

    public class AssignRequest
    {
        public int? AssignedTo { get; set; }
    }
}

