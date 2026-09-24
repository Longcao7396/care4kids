using GiveAID.Application.Features.Statistics.DTOs;

namespace GiveAID.Application.Features.Statistics.Queries.GetCauses;

/// <summary>
/// Query to get cause statistics using SQL aggregation.
/// </summary>
public record GetCausesStatsQuery : IRequest<CauseStatsDto>;
