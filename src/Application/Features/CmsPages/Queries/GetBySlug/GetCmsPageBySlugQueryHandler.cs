using GiveAID.Application.Features.CmsPages.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.CmsPages.Queries.GetBySlug;

/// <summary>
/// Handler for GetCmsPageBySlugQuery.
/// </summary>
public class GetCmsPageBySlugQueryHandler : IRequestHandler<GetCmsPageBySlugQuery, CmsPageDto>
{
    private readonly IApplicationDbContext _context;

    public GetCmsPageBySlugQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CmsPageDto> Handle(GetCmsPageBySlugQuery request, CancellationToken cancellationToken)
    {
        var page = await _context.CmsPages
            .FirstOrDefaultAsync(p => p.PageSlug == request.Slug && p.IsActive, cancellationToken);

        if (page == null)
        {
            throw new KeyNotFoundException($"CMS page with slug '{request.Slug}' not found.");
        }

        return new CmsPageDto
        {
            PageId = page.PageId,
            PageKey = page.PageKey,
            PageSlug = page.PageSlug,
            PageTitle = page.PageTitle,
            Content = page.Content,
            MetaDescription = page.MetaDescription,
            MetaKeywords = page.MetaKeywords,
            IsActive = page.IsActive,
            IsInMenu = page.IsInMenu,
            ParentPageId = page.ParentPageId,
            DisplayOrder = page.DisplayOrder
        };
    }
}
