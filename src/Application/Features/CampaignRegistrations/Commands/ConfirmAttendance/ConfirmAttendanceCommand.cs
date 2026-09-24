using MediatR;

namespace GiveAID.Application.Features.CampaignRegistrations.Commands.ConfirmAttendance;

/// <summary>
/// Command to confirm attendance for a registration.
/// </summary>
public class ConfirmAttendanceCommand : IRequest<bool>
{
    public int RegistrationId { get; set; }
}
