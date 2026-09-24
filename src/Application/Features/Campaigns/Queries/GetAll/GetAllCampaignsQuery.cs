using GiveAID.Application.Features.Campaigns.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Campaigns.Queries.GetAll;

/// <summary>
/// Query to get all campaigns.
/// </summary>
public class GetAllCampaignsQuery : IRequest<IEnumerable<CampaignDto>>
{
    public string? Status { get; set; }
    public int? CauseId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>
    /// M-05: When true, returns only campaigns that require registration (events).
    /// </summary>
    public bool EventsOnly { get; set; } = false;
}
