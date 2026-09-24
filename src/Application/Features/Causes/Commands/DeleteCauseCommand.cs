using MediatR;

namespace GiveAID.Application.Features.Causes.Commands;

/// <summary>
/// Command to delete a cause.
/// </summary>
public class DeleteCauseCommand : IRequest
{
    public int CauseId { get; }

    public DeleteCauseCommand(int causeId)
    {
        CauseId = causeId;
    }
}
