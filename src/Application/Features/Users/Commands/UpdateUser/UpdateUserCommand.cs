using GiveAID.Application.Features.Users.DTOs;
using MediatR;

namespace GiveAID.Application.Features.Users.Commands.UpdateUser;

/// <summary>
/// Command to update a user.
/// </summary>
public class UpdateUserCommand : IRequest<UserAdminDto>
{
    public int UserId { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FullName { get; set; }
    public string? Phone { get; set; }
    public string? Role { get; set; }
}
