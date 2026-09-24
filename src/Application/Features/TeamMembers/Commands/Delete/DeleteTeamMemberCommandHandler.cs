using MediatR;

namespace GiveAID.Application.Features.TeamMembers.Commands.Delete;

/// <summary>
/// Handler for DeleteTeamMemberCommand.
/// </summary>
public class DeleteTeamMemberCommandHandler : IRequestHandler<DeleteTeamMemberCommand, bool>
{
    private readonly IApplicationDbContext _context;

    public DeleteTeamMemberCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteTeamMemberCommand request, CancellationToken cancellationToken)
    {
        var member = await _context.TeamMembers.FindAsync(new object[] { request.TeamMemberId }, cancellationToken);

        if (member == null)
        {
            throw new InvalidOperationException($"Team member with ID {request.TeamMemberId} not found.");
        }

        _context.TeamMembers.Remove(member);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
