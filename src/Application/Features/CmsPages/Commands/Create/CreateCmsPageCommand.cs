using GiveAID.Application.Features.CmsPages.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CmsPages.Commands.Create;

/// <summary>
/// Command to create a CMS page.
/// </summary>
public class CreateCmsPageCommand : IRequest<CmsPageDto>
{
    public string PageKey { get; set; } = string.Empty;
    public string? PageSlug { get; set; }
    public string PageTitle { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsInMenu { get; set; } = true;
    public int? ParentPageId { get; set; }
    public int DisplayOrder { get; set; }
    public int? UpdatedBy { get; set; }
}
