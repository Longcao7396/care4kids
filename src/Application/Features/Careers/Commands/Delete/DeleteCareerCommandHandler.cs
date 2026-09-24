using MediatR;

namespace GiveAID.Application.Features.Careers.Commands.Delete;

/// <summary>
/// Handler for DeleteCareerCommand.
/// </summary>
public class DeleteCareerCommandHandler : IRequestHandler<DeleteCareerCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteCareerCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteCareerCommand request, CancellationToken cancellationToken)
    {
        var career = await _context.Careers.FindAsync(new object[] { request.CareerId }, cancellationToken);

        if (career == null)
        {
            throw new InvalidOperationException($"Career with ID {request.CareerId} not found.");
        }

        _context.Careers.Remove(career);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
