using MediatR;

namespace GiveAID.Application.Features.Campaigns.Commands.Delete;

/// <summary>
/// Command to delete a campaign.
/// </summary>
public class DeleteCampaignCommand : IRequest<bool>
{
    public int CampaignId { get; set; }
}
