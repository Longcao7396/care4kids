using MediatR;

namespace GiveAID.Application.Features.Donations.Commands.ManualConfirm;

/// <summary>
/// Command to manually confirm a donation (admin use).
/// </summary>
public class ManualConfirmCommand : IRequest<bool>
{
    public int DonationId { get; set; }
    public int ConfirmedBy { get; set; }
}
