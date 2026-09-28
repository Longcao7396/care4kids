using GiveAID.Application.Features.AdminRegistrations.DTOs;
using MediatR;

namespace GiveAID.Application.Features.AdminRegistrations.Commands.RejectRegistration;

/// <summary>
/// Admin command: mark a campaign or programme registration as Rejected.
/// </summary>
public class RejectRegistrationCommand : IRequest<AdminRegistrationDto>
{
    public string RegistrationType { get; set; } = "Campaign"; // "Campaign" or "Programme"
    public int RegistrationId { get; set; }
    public string? ReviewedBy { get; set; }
    public string? Reason { get; set; }
}