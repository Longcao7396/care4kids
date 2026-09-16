using System;
using System.Linq;
using System.Web.Http;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;

namespace GiveAID.Web.Controllers
{
    /// <summary>
    /// Help Centre â€” Frequently Asked Questions
    /// Public list (no auth) + admin CRUD.
    /// </summary>
    [RoutePrefix("api/faqs")]
    public class FaqsController : ApiController
    {
        private readonly GiveAIDContext _context;

        public FaqsController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/faqs
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll(bool activeOnly = true, string category = null, string search = null)
        {
            try
            {
                var query = _context.Faqs.AsQueryable();

                if (activeOnly)
                {
                    query = query.Where(f => f.IsActive);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(f => f.Category == category);
                }

                if (!string.IsNullOrEmpty(search))
                {
                    var term = search.Trim().ToLower();
                    query = query.Where(f =>
                        f.Question.ToLower().Contains(term) ||
                        f.Answer.ToLower().Contains(term));
                }

                var items = query
                    .OrderByDescending(f => f.IsFeatured)
                    .ThenBy(f => f.DisplayOrder)
                    .ThenBy(f => f.Question)
                    .ToList()
                    .Select(f => new
                    {
                        faqId = f.FaqId,
                        question = f.Question,
                        answer = f.Answer,
                        category = f.Category,
                        isFeatured = f.IsFeatured,
                        viewCount = f.ViewCount,
                        createdAt = f.CreatedAt
                    });

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

        // GET: api/faqs/categories
        [HttpGet]
        [Route("categories")]
        public IHttpActionResult GetCategories()
        {
            try
            {
                var categories = _context.Faqs
                    .Where(f => f.IsActive)
                    .Select(f => f.Category)
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/faqs/5
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var faq = _context.Faqs.Find(id);
                if (faq == null || !faq.IsActive)
                {
                    return NotFound();
                }

                // Increment view count
                faq.ViewCount++;
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = new
                    {
                        faqId = faq.FaqId,
                        question = faq.Question,
                        answer = faq.Answer,
                        category = faq.Category,
                        isFeatured = faq.IsFeatured,
                        viewCount = faq.ViewCount,
                        createdAt = faq.CreatedAt
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/faqs  (Admin only)
        [HttpPost]
        [Route("")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Create(Faq faq)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);
                faq.CreatedBy = userId;
                faq.CreatedAt = DateTime.Now;
                faq.UpdatedAt = DateTime.Now;
                faq.ViewCount = 0;
                faq.IsActive = true;

                _context.Faqs.Add(faq);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "FAQ created successfully",
                    Data = new
                    {
                        faqId = faq.FaqId,
                        question = faq.Question,
                        category = faq.Category
                    }
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/faqs/5  (Admin only)
        [HttpPut]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin")]
        public IHttpActionResult Update(int id, Faq faq)
        {
            try
            {
                var existing = _context.Faqs.Find(id);
                if (existing == null)
                {
                    return NotFound();
                }

                existing.Question = faq.Question;
                existing.Answer = faq.Answer;
                existing.Category = faq.Category;
                existing.DisplayOrder = faq.DisplayOrder;
                existing.IsActive = faq.IsActive;
                existing.IsFeatured = faq.IsFeatured;
                existing.UpdatedAt = DateTime.Now;

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "FAQ updated successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/faqs/5  (SuperAdmin only)
        [HttpDelete]
        [Route("{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var faq = _context.Faqs.Find(id);
                if (faq == null)
                {
                    return NotFound();
                }

                _context.Faqs.Remove(faq);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "FAQ deleted successfully"
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
}


