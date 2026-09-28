using GiveAID.Application.Features.AdminRegistrations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.AdminRegistrations.Queries.GetRegistrationStats;

/// <summary>
/// Admin query: lightweight aggregate counts of registrations, grouped by
/// status and by type. Powers the KPI chips at the top of the admin page.
/// </summary>
public class GetRegistrationStatsQuery : IRequest<AdminRegistrationStatsDto>
{
}