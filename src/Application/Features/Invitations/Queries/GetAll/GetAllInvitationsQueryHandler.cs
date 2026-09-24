using GiveAID.Application.Features.Invitations.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GiveAID.Application.Features.Invitations.Queries.GetAll;

/// <summary>
/// Handler for GetAllInvitationsQuery.
/// </summary>
public class GetAllInvitationsQueryHandler : IRequestHandler<GetAllInvitationsQuery, IEnumerable<InvitationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllInvitationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InvitationDto>> Handle(GetAllInvitationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Invitations
            .Include(i => i.Inviter)
            .AsQueryable();

        if (!string.IsNullOrEmpty(request.Status))
        {
            query = query.Where(i => i.Status == request.Status);
        }

        if (request.InviterUserId.HasValue)
        {
            query = query.Where(i => i.InviterUserId == request.InviterUserId.Value);
        }

        var invitations = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);

        return invitations.Select(i => new InvitationDto
        {
            InvitationId = i.InvitationId,
            InviterUserId = i.InviterUserId,
            InviterName = i.Inviter?.FullName,
            InviteeName = i.InviteeName,
            InviteeEmail = i.InviteeEmail,
            PersonalMessage = i.PersonalMessage,
            Status = i.Status,
            InvitationToken = i.InvitationToken,
            SentAt = i.SentAt,
            RegisteredAt = i.RegisteredAt,
            FailureReason = i.FailureReason,
            CreatedAt = i.CreatedAt
        });
    }
}
