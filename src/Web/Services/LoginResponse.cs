namespace GiveAID.Web.Services;

/// <summary>
/// DTO received from POST /api/v1/auth/login. Fields tolerate missing values
/// (e.g. when a user has not completed their FullName yet) so newly-created
/// accounts with partial profile info can still authenticate.
/// </summary>
public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}
