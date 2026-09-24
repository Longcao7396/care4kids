using GiveAID.Application.Features.Auth.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Handler for GetCurrentUserQuery.
/// 
/// SECURITY FIX #2: Returns only a minimal UserProfileDto with non-sensitive data.
/// </summary>
public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserProfileDto>
{
    private readonly IApplicationDbContext _context;

    public GetCurrentUserQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<UserProfileDto> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync(new object[] { request.UserId }, cancellationToken);

        if (user == null)
        {
            throw new InvalidOperationException("User not found.");
        }

        // SECURITY FIX #2: Map ONLY safe fields to the DTO.
        // DO NOT expose: password hash, phone, address, DOB, IsVerified,
        // IsActive, Permissions, security stamps, etc.
        return new UserProfileDto
        {
            UserId = user.UserId,
            Email = user.Email,
            FullName = user.FullName,
            Role = user.Role,
            AvatarUrl = null // Reserved for future avatar feature
        };
    }
}
