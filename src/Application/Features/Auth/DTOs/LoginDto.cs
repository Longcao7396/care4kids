namespace GiveAID.Application.Features.Auth.DTOs;

/// <summary>
/// DTO for login request — accepts username only.
/// Email is no longer a valid login identifier; users sign in with their
/// account username (case-insensitive).
/// </summary>
public class LoginDto
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}
