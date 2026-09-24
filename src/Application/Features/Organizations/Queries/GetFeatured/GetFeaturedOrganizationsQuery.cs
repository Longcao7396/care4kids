using GiveAID.Application.Features.Organizations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Organizations.Queries.GetFeatured;

/// <summary>
/// Query to get featured organizations.
/// </summary>
public class GetFeaturedOrganizationsQuery : IRequest<IEnumerable<OrganizationDto>>
{
    public int Limit { get; set; } = 6;
}
