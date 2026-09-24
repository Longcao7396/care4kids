using GiveAID.Application.Features.CmsPages.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CmsPages.Commands.Create;

/// <summary>
/// Handler for CreateCmsPageCommand.
/// </summary>
public class CreateCmsPageCommandHandler : IRequestHandler<CreateCmsPageCommand, CmsPageDto>
{
    private readonly IApplicationDbContext _context;

    public CreateCmsPageCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CmsPageDto> Handle(CreateCmsPageCommand request, CancellationToken cancellationToken)
    {
        var page = new CmsPage
        {
            PageKey = request.PageKey,
            PageSlug = request.PageSlug,
            PageTitle = request.PageTitle,
            Content = request.Content,
            MetaDescription = request.MetaDescription,
            MetaKeywords = request.MetaKeywords,
            IsActive = true,
            IsInMenu = request.IsInMenu,
            ParentPageId = request.ParentPageId,
            DisplayOrder = request.DisplayOrder,
            UpdatedBy = request.UpdatedBy,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.CmsPages.Add(page);
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
