using GiveAID.Application.Features.CampaignRegistrations.DTOs;
using GiveAID.Domain.Entities;
using MediatR;

namespace GiveAID.Application.Features.CampaignRegistrations.Commands.Register;

/// <summary>
/// Command to register for a campaign.
/// </summary>
public class RegisterCampaignCommand : IRequest<RegistrationDto>
{
    public int CampaignId { get; set; }
    public int UserId { get; set; }
    public string? Notes { get; set; }
}
