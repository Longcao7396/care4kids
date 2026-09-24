using GiveAID.Application.Features.Organizations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Organizations.Queries.GetFeatured;

/// <summary>
/// Handler for GetFeaturedOrganizationsQuery.
/// </summary>
public class GetFeaturedOrganizationsQueryHandler : IRequestHandler<GetFeaturedOrganizationsQuery, IEnumerable<OrganizationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetFeaturedOrganizationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OrganizationDto>> Handle(GetFeaturedOrganizationsQuery request, CancellationToken cancellationToken)
    {
        var organizations = await _context.Organizations
            .Where(o => o.IsFeatured && o.IsActive)
            .OrderBy(o => o.DisplayOrder)
            .Take(request.Limit)
            .ToListAsync(cancellationToken);

        return organizations.Select(o => new OrganizationDto
        {
            OrganizationId = o.OrganizationId,
            OrganizationName = o.OrganizationName,
            OrganizationType = o.OrganizationType,
            Description = o.Description,
            LogoUrl = o.LogoUrl,
            WebsiteUrl = o.WebsiteUrl,
            ContactEmail = o.ContactEmail,
            ContactPhone = o.ContactPhone,
            Address = o.Address,
            Mission = o.Mission,
            Vision = o.Vision,
            ContributionAmount = o.ContributionAmount,
            ContributionType = o.ContributionType,
            IsActive = o.IsActive,
            IsFeatured = o.IsFeatured,
            DisplayOrder = o.DisplayOrder
        });
    }
}
