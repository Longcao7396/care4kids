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
    /// CMS pages controller â€” exposes the existing CmsPages table so the
    /// About Us module can pull editable text content (about_us, our_team,
    /// careers, achievements, contact_info, etc.) and admins can manage it.
    ///
    /// Full CRUD: GET (public), POST/PUT/DELETE (admin).
    /// </summary>
    [RoutePrefix("api/cms")]
    public class CmsPagesController : ApiController
    {
        private readonly GiveAIDContext _context;

        public CmsPagesController()
        {
            _context = new GiveAIDContext();
        }

        // GET: api/cms/pages?keys=about_us,contact_info
        [HttpGet]
        [Route("pages")]
        public IHttpActionResult GetPages(string keys = null, bool includeInactive = false)
        {
            try
            {
                var query = _context.CmsPages.AsQueryable();

                if (!includeInactive)
                {
                    query = query.Where(p => p.IsActive);
                }

                if (!string.IsNullOrEmpty(keys))
                {
                    var keyList = keys
                        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(k => k.Trim())
                        .ToList();
                    query = query.Where(p => keyList.Contains(p.PageKey));
                }

                var pages = query
                    .OrderBy(p => p.DisplayOrder)
                    .ToList()
                    .Select(ToDto);

                return Ok(new ApiResponse { Success = true, Data = pages });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/cms/pages/about_us
        [HttpGet]
        [Route("pages/{key}")]
        public IHttpActionResult GetByKey(string key)
        {
            try
            {
                var page = _context.CmsPages.FirstOrDefault(p => p.PageKey == key && p.IsActive);
                if (page == null)
                {
                    return NotFound();
                }

                return Ok(new ApiResponse
                {
                    Success = true,
                    Data = ToDto(page)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // GET: api/cms/pages/id/5  (Admin only â€” includes inactive)
        [HttpGet]
        [Route("pages/id/{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin,ContentManager")]
        public IHttpActionResult GetById(int id)
        {
            try
            {
                var page = _context.CmsPages.Find(id);
                if (page == null) return NotFound();
                return Ok(new ApiResponse { Success = true, Data = ToDto(page) });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // POST: api/cms/pages  (Admin only)
        [HttpPost]
        [Route("pages")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin,ContentManager")]
        public IHttpActionResult Create(CmsPageCreateRequest request)
        {
            try
            {
                if (request == null ||
                    string.IsNullOrWhiteSpace(request.PageKey) ||
                    string.IsNullOrWhiteSpace(request.PageTitle))
                {
                    return BadRequest("pageKey and pageTitle are required.");
                }

                var key = request.PageKey.Trim().ToLowerInvariant();

                if (_context.CmsPages.Any(p => p.PageKey == key))
                {
                    return BadRequest("A page with this key already exists.");
                }

                var userId = JwtHelper.GetUserIdFromToken(Request);

                var page = new CmsPage
                {
                    PageKey = key,
                    PageSlug = string.IsNullOrWhiteSpace(request.PageSlug)
                        ? key
                        : request.PageSlug.Trim().ToLowerInvariant(),
                    PageTitle = request.PageTitle.Trim(),
                    Content = request.Content,
                    MetaDescription = request.MetaDescription,
                    MetaKeywords = request.MetaKeywords,
                    IsActive = true,
                    IsInMenu = request.IsInMenu ?? true,
                    DisplayOrder = request.DisplayOrder ?? 100,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    UpdatedBy = userId,
                };

                _context.CmsPages.Add(page);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Page created successfully.",
                    Data = ToDto(page)
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // PUT: api/cms/pages/5  (Admin only)
        [HttpPut]
        [Route("pages/{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin,Admin,ContentManager")]
        public IHttpActionResult Update(int id, CmsPageUpdateRequest request)
        {
            try
            {
                var page = _context.CmsPages.Find(id);
                if (page == null)
                {
                    return NotFound();
                }

                if (request.PageTitle != null) page.PageTitle = request.PageTitle;
                if (request.Content != null) page.Content = request.Content;
                if (request.MetaDescription != null) page.MetaDescription = request.MetaDescription;
                if (request.MetaKeywords != null) page.MetaKeywords = request.MetaKeywords;
                if (request.IsInMenu.HasValue) page.IsInMenu = request.IsInMenu.Value;
                if (request.IsActive.HasValue) page.IsActive = request.IsActive.Value;
                if (request.DisplayOrder.HasValue) page.DisplayOrder = request.DisplayOrder.Value;

                page.UpdatedAt = DateTime.Now;
                try
                {
                    page.UpdatedBy = JwtHelper.GetUserIdFromToken(Request);
                }
                catch
                {
                    // Non-fatal: the user identity isn't strictly required.
                }

                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Page updated successfully"
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        // DELETE: api/cms/pages/5  (SuperAdmin only)
        [HttpDelete]
        [Route("pages/{id:int}")]
        [JwtAuthorize(Roles = "SuperAdmin")]
        public IHttpActionResult Delete(int id)
        {
            try
            {
                var page = _context.CmsPages.Find(id);
                if (page == null) return NotFound();

                _context.CmsPages.Remove(page);
                _context.SaveChanges();

                return Ok(new ApiResponse
                {
                    Success = true,
                    Message = "Page deleted successfully."
                });
            }
            catch (Exception ex)
            {
                return InternalServerError(ex);
            }
        }

        private static object ToDto(CmsPage p)
        {
            return new
            {
                pageId = p.PageId,
                pageKey = p.PageKey,
                pageSlug = p.PageSlug,
                pageTitle = p.PageTitle,
                content = p.Content,
                metaDescription = p.MetaDescription,
                metaKeywords = p.MetaKeywords,
                isActive = p.IsActive,
                isInMenu = p.IsInMenu,
                displayOrder = p.DisplayOrder,
                createdAt = p.CreatedAt,
                updatedAt = p.UpdatedAt,
                updatedBy = p.UpdatedBy
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

    public class CmsPageUpdateRequest
    {
        public string PageTitle { get; set; }
        public string Content { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public bool? IsInMenu { get; set; }
        public bool? IsActive { get; set; }
        public bool? IsFeatured { get; set; }
        public int? DisplayOrder { get; set; }
    }

    public class CmsPageCreateRequest
    {
        public string PageKey { get; set; }
        public string PageSlug { get; set; }
        public string PageTitle { get; set; }
        public string Content { get; set; }
        public string MetaDescription { get; set; }
        public string MetaKeywords { get; set; }
        public bool? IsInMenu { get; set; }
        public int? DisplayOrder { get; set; }
    }
}


