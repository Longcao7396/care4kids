using GiveAID.Application.Features.CmsPages.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CmsPages.Queries.GetBySlug;

/// <summary>
/// Query to get a CMS page by slug.
/// </summary>
public class GetCmsPageBySlugQuery : IRequest<CmsPageDto>
{
    public string Slug { get; set; } = string.Empty;
}
