using GiveAID.Application.Features.CmsPages.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CmsPages.Commands.Update;

/// <summary>
/// Handler for UpdateCmsPageCommand.
/// </summary>
public class UpdateCmsPageCommandHandler : IRequestHandler<UpdateCmsPageCommand, CmsPageDto>
{
    private readonly IApplicationDbContext _context;

    public UpdateCmsPageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CmsPageDto> Handle(UpdateCmsPageCommand request, CancellationToken cancellationToken)
    {
        var page = await _context.CmsPages.FindAsync(new object[] { request.PageId }, cancellationToken);

        if (page == null)
        {
            throw new InvalidOperationException($"CMS page with ID {request.PageId} not found.");
        }

        if (request.PageKey != null) page.PageKey = request.PageKey;
        if (request.PageSlug != null) page.PageSlug = request.PageSlug;
        if (request.PageTitle != null) page.PageTitle = request.PageTitle;
        if (request.Content != null) page.Content = request.Content;
        if (request.MetaDescription != null) page.MetaDescription = request.MetaDescription;
        if (request.MetaKeywords != null) page.MetaKeywords = request.MetaKeywords;
        if (request.IsActive.HasValue) page.IsActive = request.IsActive.Value;
        if (request.IsInMenu.HasValue) page.IsInMenu = request.IsInMenu.Value;
        if (request.ParentPageId.HasValue) page.ParentPageId = request.ParentPageId;
        if (request.DisplayOrder.HasValue) page.DisplayOrder = request.DisplayOrder.Value;
        if (request.UpdatedBy.HasValue) page.UpdatedBy = request.UpdatedBy;
        page.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

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
