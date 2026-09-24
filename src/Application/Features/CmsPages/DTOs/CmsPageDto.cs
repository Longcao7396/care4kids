namespace GiveAID.Application.Features.CmsPages.DTOs;

/// <summary>
/// DTO for CMS page data.
/// </summary>
public class CmsPageDto
{
    public int PageId { get; set; }
    public string PageKey { get; set; } = string.Empty;
    public string? PageSlug { get; set; }
    public string PageTitle { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? MetaDescription { get; set; }
    public string? MetaKeywords { get; set; }
    public bool IsActive { get; set; }
    public bool IsInMenu { get; set; }
    public int? ParentPageId { get; set; }
    public int DisplayOrder { get; set; }
}
