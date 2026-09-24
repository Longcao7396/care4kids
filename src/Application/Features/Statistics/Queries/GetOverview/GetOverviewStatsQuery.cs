using GiveAID.Application.Features.Statistics.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Statistics.Queries.GetOverview;

/// <summary>
/// Query to get overview statistics.
/// </summary>
public class GetOverviewStatsQuery : IRequest<OverviewStatsDto>
{
}
