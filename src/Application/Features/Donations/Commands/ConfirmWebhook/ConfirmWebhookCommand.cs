using MediatR;

namespace GiveAID.Application.Features.Donations.Commands.ConfirmWebhook;

/// <summary>
/// Command to confirm a donation via payment gateway webhook.
/// </summary>
public class ConfirmWebhookCommand : IRequest<bool>
{
    public string Gateway { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string Signature { get; set; } = string.Empty;
}
