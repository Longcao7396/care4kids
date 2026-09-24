using GiveAID.Application.Features.Statistics.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Statistics.Queries.GetDashboard;

/// <summary>
/// Query to get dashboard statistics.
/// </summary>
public class GetDashboardStatsQuery : IRequest<DashboardStatsDto>
{
}
