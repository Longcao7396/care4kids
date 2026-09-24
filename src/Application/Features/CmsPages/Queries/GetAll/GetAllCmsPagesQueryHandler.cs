using GiveAID.Application.Features.CmsPages.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.CmsPages.Queries.GetAll;

/// <summary>
/// Handler for GetAllCmsPagesQuery.
/// </summary>
public class GetAllCmsPagesQueryHandler : IRequestHandler<GetAllCmsPagesQuery, IEnumerable<CmsPageDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllCmsPagesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CmsPageDto>> Handle(GetAllCmsPagesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.CmsPages.AsQueryable();

        if (request.ActiveOnly)
        {
            query = query.Where(p => p.IsActive);
        }

        var pages = await query
            .OrderBy(p => p.DisplayOrder)
            .ThenBy(p => p.PageTitle)
            .ToListAsync(cancellationToken);

        return pages.Select(p => new CmsPageDto
        {
            PageId = p.PageId,
            PageKey = p.PageKey,
            PageSlug = p.PageSlug,
            PageTitle = p.PageTitle,
            Content = p.Content,
            MetaDescription = p.MetaDescription,
            MetaKeywords = p.MetaKeywords,
            IsActive = p.IsActive,
            IsInMenu = p.IsInMenu,
            ParentPageId = p.ParentPageId,
            DisplayOrder = p.DisplayOrder
        });
    }
}
