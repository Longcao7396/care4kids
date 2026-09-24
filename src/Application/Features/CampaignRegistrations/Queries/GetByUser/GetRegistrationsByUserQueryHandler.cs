using GiveAID.Application.Features.CampaignRegistrations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.CampaignRegistrations.Queries.GetByUser;

/// <summary>
/// Handler for GetRegistrationsByUserQuery.
/// </summary>
public class GetRegistrationsByUserQueryHandler : IRequestHandler<GetRegistrationsByUserQuery, IEnumerable<RegistrationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRegistrationsByUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<RegistrationDto>> Handle(GetRegistrationsByUserQuery request, CancellationToken cancellationToken)
    {
        var registrations = await _context.CampaignRegistrations
            .Include(r => r.Campaign)
            .Include(r => r.User)
            .Where(r => r.UserId == request.UserId)
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
