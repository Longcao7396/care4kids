using GiveAID.Application.Features.CampaignRegistrations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.CampaignRegistrations.Queries.GetByCampaign;

/// <summary>
/// Handler for GetRegistrationsByCampaignQuery.
/// </summary>
public class GetRegistrationsByCampaignQueryHandler : IRequestHandler<GetRegistrationsByCampaignQuery, IEnumerable<RegistrationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRegistrationsByCampaignQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegistrationDto>> Handle(GetRegistrationsByCampaignQuery request, CancellationToken cancellationToken)
    {
        var registrations = await _context.CampaignRegistrations
            .Include(r => r.Campaign)
            .Include(r => r.User)
            .Where(r => r.CampaignId == request.CampaignId)
            .OrderByDescending(r => r.RegistrationDate)
            .ToListAsync(cancellationToken);

        return registrations.Select(r => new RegistrationDto
        {
            RegistrationId = r.RegistrationId,
            CampaignId = r.CampaignId,
            CampaignName = r.Campaign?.CampaignName,
            UserId = r.UserId,
            UserName = r.User?.FullName,
            Status = r.Status,
            Notes = r.Notes,
            AttendanceConfirmed = r.AttendanceConfirmed,
            RegistrationDate = r.RegistrationDate
        });
    }
}
