using GiveAID.Application.Features.AdminRegistrations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.AdminRegistrations.Commands.ApproveRegistration;

/// <summary>
/// Admin command: mark a campaign or programme registration as Approved.
/// </summary>
public class ApproveRegistrationCommand : IRequest<AdminRegistrationDto>
{
    public string RegistrationType { get; set; } = "Campaign"; // "Campaign" or "Programme"
    public int RegistrationId { get; set; }
    public string? ReviewedBy { get; set; }
}