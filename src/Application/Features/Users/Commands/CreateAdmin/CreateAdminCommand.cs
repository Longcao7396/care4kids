using GiveAID.Application.Features.Users.DTOs;
using GiveAID.Domain.Entities;
using GiveAID.Application.Services;
using MediatR;

namespace GiveAID.Application.Features.Users.Commands.CreateAdmin;

/// <summary>
/// Command to create an admin user.
/// </summary>
public class CreateAdminCommand : IRequest<UserAdminDto>
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = "Admin";
    public string? Phone { get; set; }
}
