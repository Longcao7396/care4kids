using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using GiveAID.Application.Services;
using GiveAID.Infrastructure.Auth;
using Microsoft.Extensions.Logging;

namespace GiveAID.Infrastructure.Security;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<JwtTokenService>? _logger;

    public JwtTokenService(
        IOptions<JwtSettings> settings,
        GiveAID.Application.Common.Interfaces.IApplicationDbContext dbContext,
        ILogger<JwtTokenService>? logger = null)
    {
        _settings = settings.Value;
        _dbContext = dbContext;
        _logger = logger;
    }

    public string GenerateToken(int userId, string email, string role, string? username = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        // Get user for password_changed_at claim
        var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
        long? pwdAtUnix = user?.PasswordChangedAt.HasValue == true
            ? (long)(user.PasswordChangedAt.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
            : null;

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Role, role),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new("password_at", (pwdAtUnix ?? 0).ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrEmpty(username))
        {
            claims.Add(new Claim(ClaimTypes.Name, username));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public int? ValidateToken(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_settings.Secret);

        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = _settings.Issuer,
            ValidateAudience = true,
            ValidAudience = _settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        try
        {
            var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

            // Check if user still exists and is active
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                var user = _dbContext.Users.FirstOrDefault(u => u.UserId == userId);
                if (user == null || !user.IsActive)
                {
                    return null;
                }

                // Check password_at claim
                var pwdAtClaim = principal?.FindFirst("password_at")?.Value;
                long pwdAtFromToken = 0;
                long.TryParse(pwdAtClaim, out pwdAtFromToken);

                long pwdAtFromDb = user.PasswordChangedAt.HasValue
                    ? (long)(user.PasswordChangedAt.Value.ToUniversalTime() - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds
                    : 0;

                if (pwdAtFromToken < pwdAtFromDb)
                {
                    return null; // Token issued before password change
                }

                return userId;
            }

            return null;
        }
        catch (SecurityTokenException ex)
        {
            _logger?.LogWarning("JWT validation failed: {Reason}", ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            _logger?.LogWarning("Unexpected error during JWT validation: {Error}", ex.Message);
            return null;
        }
    }

    public DateTime GetTokenExpiration()
    {
        return DateTime.UtcNow.AddMinutes(_settings.ExpiryMinutes);
    }

    public int? GetUserIdFromToken(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            return userIdClaim != null && int.TryParse(userIdClaim.Value, out var userId) ? userId : null;
        }
        catch
        {
            return null;
        }
    }
}
