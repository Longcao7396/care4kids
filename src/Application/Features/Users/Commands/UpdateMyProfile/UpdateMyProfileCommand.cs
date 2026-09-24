using GiveAID.Application.Features.Auth.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Users.Commands.UpdateMyProfile;

/// <summary>
/// Command to update the current user's own profile.
/// </summary>
public class UpdateMyProfileCommand : IRequest<UserDto>
{
    public int UserId { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}
