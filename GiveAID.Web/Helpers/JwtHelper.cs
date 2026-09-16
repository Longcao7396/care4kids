using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using GiveAID.Web.Models;
using GiveAID.Web.Data;

namespace GiveAID.Web.Helpers
{
    public static class JwtHelper
    {
        // Cached GiveAIDContext instance used for token validation lookups.
        // Disposed when the AppDomain shuts down (one per process lifetime is fine).
        private static readonly GiveAIDContext _validationContext = new GiveAIDContext();

        public static string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSettings.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: JwtSettings.Issuer,
                audience: JwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(JwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public static int GetUserIdFromToken(HttpRequestMessage request)
        {
            try
            {
                var authHeader = request.Headers.Authorization;
                if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
                {
                    throw new UnauthorizedAccessException("No authorization header");
                }

                var token = authHeader.Parameter;
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    throw new UnauthorizedAccessException("Invalid token");
                }

                return int.Parse(userIdClaim.Value);
            }
            catch
            {
                throw new UnauthorizedAccessException("Invalid token");
            }
        }

        public static ClaimsPrincipal ValidateToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(JwtSettings.Secret);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = JwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = JwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);

                // SECURITY: Reject tokens belonging to deactivated users so that an
                // admin disabling an account immediately invalidates all outstanding
                // tokens for that user (until JWT expiry).
                var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
                {
                    var user = _validationContext.Users.Find(userId);
                    if (user == null || !user.IsActive)
                    {
                        return null;
                    }
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Validates the request's JWT and returns true if the caller holds the
        /// SuperAdmin or Admin role. Centralizes the role-check pattern used by
        /// admin-only controllers. Returns false (does not throw) when the
        /// token is missing, invalid, or the role doesn't match — callers
        /// should map the return value to a 401/403 response.
        /// </summary>
        public static bool CheckAdmin(HttpRequestMessage request)
        {
            try
            {
                var authHeader = request.Headers.Authorization;
                if (authHeader == null || string.IsNullOrEmpty(authHeader.Parameter))
                    return false;

                var principal = ValidateToken(authHeader.Parameter);
                if (principal == null) return false;

                var role = principal.FindFirst(ClaimTypes.Role)?.Value;
                return role == "SuperAdmin" || role == "Admin";
            }
            catch
            {
                return false;
            }
        }
    }
}
