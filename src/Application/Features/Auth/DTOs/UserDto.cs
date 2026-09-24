namespace GiveAID.Application.Features.Auth.DTOs;

/// <summary>
/// DTO representing a user.
/// </summary>
public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public string? Phone { get; set; }
    public string? Profession { get; set; }
    public string? Address { get; set; }
}
