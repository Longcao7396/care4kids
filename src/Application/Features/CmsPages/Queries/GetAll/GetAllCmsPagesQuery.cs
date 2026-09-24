using GiveAID.Application.Features.CmsPages.DTOs;
using MediatR;

namespace GiveAID.Application.Features.CmsPages.Queries.GetAll;

/// <summary>
/// Query to get all CMS pages.
/// </summary>
public class GetAllCmsPagesQuery : IRequest<IEnumerable<CmsPageDto>>
{
    public bool ActiveOnly { get; set; } = false;
}
