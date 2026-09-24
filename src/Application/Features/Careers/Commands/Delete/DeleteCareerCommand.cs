using MediatR;

namespace GiveAID.Application.Features.Careers.Commands.Delete;

/// <summary>
/// Command to delete a career listing.
/// </summary>
public class DeleteCareerCommand : IRequest<bool>
{
    public int CareerId { get; set; }
}
