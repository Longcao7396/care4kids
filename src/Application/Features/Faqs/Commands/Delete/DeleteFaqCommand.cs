using MediatR;

namespace GiveAID.Application.Features.Faqs.Commands.Delete;

/// <summary>
/// Command to delete a FAQ.
/// </summary>
public class DeleteFaqCommand : IRequest<bool>
{
    public int FaqId { get; set; }
}
