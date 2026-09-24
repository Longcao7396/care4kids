using GiveAID.Application.Features.Auth.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Auth.Queries.GetCurrentUser;

/// <summary>
/// Query to get the current authenticated user's profile.
/// </summary>
public class GetCurrentUserQuery : IRequest<UserProfileDto>
{
    public int UserId { get; set; }
}
