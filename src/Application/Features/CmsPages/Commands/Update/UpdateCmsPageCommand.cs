using GiveAID.Application.Features.CmsPages.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CmsPages.Commands.Update;

/// <summary>
/// Command to update a CMS page.
/// </summary>
public class UpdateCmsPageCommand : IRequest<CmsPageDto>
{
    public int PageId { get; set; }
    public string? PageKey { get; set; }
    public string? PageSlug { get; set; }
    public string? PageTitle { get; set; }
    public string? Content { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsInMenu { get; set; }
    public int? ParentPageId { get; set; }
    public int? DisplayOrder { get; set; }
    public int? UpdatedBy { get; set; }
}
