namespace GiveAID.Application.Features.Users.DTOs;

/// <summary>
/// DTO for updating user role.
/// </summary>
public class UpdateUserRoleDto
{
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating user status.
/// </summary>
public class UpdateUserStatusDto
{
    public bool IsActive { get; set; }
}
