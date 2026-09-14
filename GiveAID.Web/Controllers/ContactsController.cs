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
    /// Contact form submissions (ContactMessages table).
    /// Public POST for submitting + Admin read/list/reply.
    /// </summary>
    [RoutePrefix("api/contacts")]
    public class ContactsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public ContactsController()
        {
            _context = new GiveAIDContext();
        }

        // POST: api/contacts  — PUBLIC: submit contact form
        [HttpPost]
        [Route("")]
        public IHttpActionResult Submit(ContactSubmitRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.Message))
                {
                    return BadRequest("Name, email and message are required.");
                }

                if (!new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(request.Email))
                {
                    return BadRequest("Please provide a valid email address.");
                }

                var contact = new ContactMessage
                {
                    Name = request.Name.Trim(),
                    Email = request.Email.Trim(),
                    Phone = request.Phone?.Trim(),
                    Subject = request.Subject?.Trim(),
                    Message = request.Message.Trim(),
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };

                _context.ContactMessages.Add(contact);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Thank you for contacting us! We will get back to you within 2 business days.",
                    Data = new
                    {
                        contactId = contact.ContactId,
                        submittedAt = contact.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/contacts  — Admin: list all submissions
        [HttpGet]
        [Route("")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetAll(
            bool? isRead = null,
            string search = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.ContactMessages.AsQueryable();

                if (isRead.HasValue)
                {
                    query = query.Where(c => c.IsRead == isRead.Value);
                }

                if (!string.IsNullOrWhiteSpace(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(c =>
                        c.Name.ToLower().Contains(term) ||
                        c.Email.ToLower().Contains(term) ||
                        (c.Subject != null && c.Subject.ToLower().Contains(term)));
                }

                var total = query.Count();

                var items = query
                    .OrderByDescending(c => c.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList()
                    .Select(c => new
                    {
                        contactId = c.ContactId,
                        name = c.Name,
                        email = c.Email,
                        phone = c.Phone,
                        subject = c.Subject,
                        messagePreview = c.Message.Length > 80
                            ? c.Message.Substring(0, 80) + "…"
                            : c.Message,
                        isRead = c.IsRead,
                        hasReply = !string.IsNullOrEmpty(c.ReplyMessage),
                        createdAt = c.CreatedAt
                    });

                return Ok(new
                {
                    success = true,
                    data = items,
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

        // GET: api/contacts/5  — Admin: get single submission
        [HttpGet]
        [Route("{id:int}")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var contact = _context.ContactMessages.Find(id);
                if (contact == null)
                {
                    return NotFound();
                }

                // Mark as read on first admin view
                if (!contact.IsRead)
                {
                    contact.IsRead = true;
                    _context.SaveChanges();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        contactId = contact.ContactId,
                        name = contact.Name,
                        email = contact.Email,
                        phone = contact.Phone,
                        subject = contact.Subject,
                        message = contact.Message,
                        isRead = contact.IsRead,
                        replyMessage = contact.ReplyMessage,
                        repliedAt = contact.RepliedAt,
                        createdAt = contact.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/contacts/5/reply  — Admin: send reply
        [HttpPut]
        [Route("{id:int}/reply")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Reply(int id, ContactReplyRequest request)
        {
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ReplyMessage))
                {
                    return BadRequest("Reply message is required.");
                }

                var contact = _context.ContactMessages.Find(id);
                if (contact == null)
                {
                    return NotFound();
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);

                contact.ReplyMessage = request.ReplyMessage.Trim();
                contact.RepliedAt = DateTime.Now;
                contact.RepliedBy = userId;
                contact.IsRead = true; // also mark as read

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Reply sent successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/contacts/5/read  — Admin: toggle read/unread
        [HttpPut]
        [Route("{id:int}/read")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult ToggleRead(int id)
        {
            try
            {
                var contact = _context.ContactMessages.Find(id);
                if (contact == null)
                {
                    return NotFound();
                }

                contact.IsRead = !contact.IsRead;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = contact.IsRead ? "Marked as read." : "Marked as unread."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/contacts/5  — SuperAdmin: hard delete
        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var contact = _context.ContactMessages.Find(id);
                if (contact == null)
                {
                    return NotFound();
                }

                _context.ContactMessages.Remove(contact);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Contact submission deleted."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/contacts/stats  — Admin: unread count
        [HttpGet]
        [Route("stats")]
        [Authorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult GetStats()
        {
            try
            {
                var total = _context.ContactMessages.Count();
                var unread = _context.ContactMessages.Count(c => !c.IsRead);
                var replied = _context.ContactMessages.Count(c => !string.IsNullOrEmpty(c.ReplyMessage));

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new { total, unread, replied }
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

    public class ContactSubmitRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
    }

    public class ContactReplyRequest
    {
        public string ReplyMessage { get; set; }
    }
}
