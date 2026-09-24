namespace GiveAID.Application.Services;

/// <summary>
/// Interface for JWT token generation and validation.
/// Implementation provided by Infrastructure layer.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT token for the specified user.
    /// </summary>
    string GenerateToken(int userId, string email, string username, string role);

    /// <summary>
    /// Validates a JWT token and returns the user ID if valid.
    /// </summary>
    int? ValidateToken(string token);

    /// <summary>
    /// Gets the expiration time for a newly generated token.
    /// </summary>
    DateTime GetTokenExpiration();
}
