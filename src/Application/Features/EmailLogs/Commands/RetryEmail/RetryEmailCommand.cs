using MediatR;

namespace GiveAID.Application.Features.EmailLogs.Commands.RetryEmail;

/// <summary>
/// Command to retry a failed email.
/// </summary>
public class RetryEmailCommand : IRequest<bool>
{
    public int EmailLogId { get; set; }
}
