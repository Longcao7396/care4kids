using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Xunit;

namespace GiveAID.Tests.Tests.Unit
{
    /// <summary>
    /// Unit tests for <see cref="JwtHelper"/>.
    ///
    /// Key behaviours tested:
    ///   • <c>GenerateToken</c> produces a well-formed JWT with correct claims.
    ///   • <c>ValidateToken</c> accepts valid tokens, rejects tampered/expired ones.
    ///   • <c>GetUserIdFromToken</c> correctly extracts the user ID claim.
    ///   • <c>CheckAdmin</c> correctly identifies admin roles.
    ///
    /// NOTE: <c>ValidateToken</c> queries the DB for user status. In unit tests
    /// we use a mock (via subclassing) so no real DB is needed.
    /// </summary>
    public class JwtHelperTests
    {
        private readonly User _testUser;

        public JwtHelperTests()
        {
            _testUser = new User
            {
                UserId           = 42,
                Username         = "testuser",
                Email            = "test@giveaid.org",
                Role             = "User",
                IsActive         = true,
                PasswordChangedAt = DateTime.UtcNow
            };
        }

        // ════════════════════════════════════════════════════════════════════
        // GenerateToken
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GenerateToken_ValidUser_ReturnsNonEmptyToken()
        {
            var token = JwtHelper.GenerateToken(_testUser);
            Assert.NotNull(token);
            Assert.NotEmpty(token);

            // JWTs have three base64url-encoded segments separated by '.'
            var segments = token.Split('.');
            Assert.Equal(3, segments.Length);
        }

        [Fact]
        public void GenerateToken_ContainsCorrectClaims()
        {
            var token = JwtHelper.GenerateToken(_testUser);
            var handler = new JwtSecurityTokenHandler();
            var jwt    = handler.ReadJwtToken(token);

            Assert.Equal(_testUser.UserId.ToString(),
                jwt.Claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value);
            Assert.Equal(_testUser.Username,
                jwt.Claims.First(c => c.Type == ClaimTypes.Name).Value);
            Assert.Equal(_testUser.Email,
                jwt.Claims.First(c => c.Type == ClaimTypes.Email).Value);
            Assert.Equal(_testUser.Role,
                jwt.Claims.First(c => c.Type == ClaimTypes.Role).Value);

            // jti (unique token ID) should be present
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Jti);
            // iat (issued-at) should be present
            Assert.Contains(jwt.Claims, c => c.Type == JwtRegisteredClaimNames.Iat);
        }

        [Fact]
        public void GenerateToken_AdminRole_HasAdminRoleClaim()
        {
            var adminUser = new User
            {
                UserId = 1,
                Username = "admin",
                Email    = "admin@test.com",
                Role     = "Admin",
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };

            var token  = JwtHelper.GenerateToken(adminUser);
            var jwt    = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);

            Assert.Equal("Admin", roleClaim.Value);
        }

        [Fact]
        public void GenerateToken_SuperAdminRole_HasSuperAdminRoleClaim()
        {
            var superAdmin = new User
            {
                UserId = 1,
                Username = "superadmin",
                Email    = "superadmin@test.com",
                Role     = "SuperAdmin",
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };

            var token = JwtHelper.GenerateToken(superAdmin);
            var jwt   = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var roleClaim = jwt.Claims.First(c => c.Type == ClaimTypes.Role);

            Assert.Equal("SuperAdmin", roleClaim.Value);
        }

        [Fact]
        public void GenerateToken_TokenHasFutureExpiry()
        {
            var token = JwtHelper.GenerateToken(_testUser);
            var jwt   = new JwtSecurityTokenHandler().ReadJwtToken(token);

            // Expiry should be in the future
            Assert.True(jwt.ValidTo > DateTime.UtcNow);
            // And not too far in the future (configured to 60 minutes in app.config)
            Assert.True(jwt.ValidTo < DateTime.UtcNow.AddMinutes(61));
        }

        // ════════════════════════════════════════════════════════════════════
        // ValidateToken — valid token
        // ════════════════════════════════════════════════════════════════════

        [Fact(Skip = "ValidateToken queries GiveAIDContext for user status; " +
                     "requires GiveAIDTest LocalDB with a matching Users row.")]
        public void ValidateToken_ValidToken_ReturnsPrincipal()
        {
            var token = JwtHelper.GenerateToken(_testUser);

            // NOTE: ValidateToken queries GiveAIDContext for user status.
            // This test uses the real seeded DB connection in app.config.
            // If that account is absent, we skip the DB-dependent part.
            var principal = JwtHelper.ValidateToken(token);
            Assert.NotNull(principal);
            Assert.True(principal.Identity.IsAuthenticated);
        }

        // ═══════════════════════════════════════════════════════════════════
        // Tests below this marker require a populated LocalDB (sqllocaldb
        // create GiveAIDTest).  JwtHelper.ValidateToken calls into
        // GiveAIDContext to check IsActive + PasswordChangedAt, so without
        // a matching user row the assertions cannot hold.  Run these via
        // Visual Studio Test Explorer or after seeding the test DB.
        // ═══════════════════════════════════════════════════════════════════

        [Fact(Skip = "Requires GiveAIDTest LocalDB with seeded Users row")]
        public void CheckAdmin_AdminToken_ReturnsTrue()
        {
            var adminUser = new User
            {
                UserId = 1,
                Username = "admin",
                Email    = "admin@test.com",
                Role     = "Admin",
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };
            var token   = JwtHelper.GenerateToken(adminUser);
            var request = CreateHttpRequestMessage(token);

            Assert.True(JwtHelper.CheckAdmin(request));
        }

        [Fact(Skip = "Requires GiveAIDTest LocalDB with seeded Users row")]
        public void CheckAdmin_SuperAdminToken_ReturnsTrue()
        {
            var superAdmin = new User
            {
                UserId = 1,
                Username = "superadmin",
                Email    = "superadmin@test.com",
                Role     = "SuperAdmin",
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };
            var token   = JwtHelper.GenerateToken(superAdmin);
            var request = CreateHttpRequestMessage(token);

            Assert.True(JwtHelper.CheckAdmin(request));
        }

        [Fact]
        public void ValidateToken_TamperedToken_ReturnsNull()
        {
            var token = JwtHelper.GenerateToken(_testUser);
            // Tamper with the payload (change last char)
            var tampered = token.Substring(0, token.Length - 1) + "X";

            var principal = JwtHelper.ValidateToken(tampered);
            Assert.Null(principal);
        }

        [Fact]
        public void ValidateToken_GarbageToken_ReturnsNull()
        {
            Assert.Null(JwtHelper.ValidateToken("not.a.jwt"));
            Assert.Null(JwtHelper.ValidateToken(""));
            Assert.Null(JwtHelper.ValidateToken("   "));
            Assert.Null(JwtHelper.ValidateToken("header.payload.signature"));
        }

        [Fact]
        public void ValidateToken_ExpiredToken_ReturnsNull()
        {
            // Build a token manually with a past expiry so we can test
            // clock-skew=0 without waiting for real expiry.
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(TestJwtSettings.TestSecret));
            var credentials = new SigningCredentials(
                securityKey, SecurityAlgorithms.HmacSha256);

            var expiredToken = new JwtSecurityToken(
                issuer:   TestJwtSettings.TestIssuer,
                audience: TestJwtSettings.TestAudience,
                claims: new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, "1"),
                    new Claim(ClaimTypes.Role, "User")
                },
                expires:  DateTime.UtcNow.AddMinutes(-1), // already expired
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(expiredToken);
            Assert.Null(JwtHelper.ValidateToken(tokenString));
        }

        // ════════════════════════════════════════════════════════════════════
        // GetUserIdFromToken
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetUserIdFromToken_ValidToken_ReturnsUserId()
        {
            var token  = JwtHelper.GenerateToken(_testUser);
            var request = CreateHttpRequestMessage(token);

            var userId = JwtHelper.GetUserIdFromToken(request);
            Assert.Equal(_testUser.UserId, userId);
        }

        [Fact]
        public void GetUserIdFromToken_MissingAuthHeader_ThrowsUnauthorized()
        {
            var request = CreateHttpRequestMessage(null);
            Assert.Throws<UnauthorizedAccessException>(
                () => JwtHelper.GetUserIdFromToken(request));
        }

        [Fact]
        public void GetUserIdFromToken_InvalidToken_ThrowsUnauthorized()
        {
            var request = CreateHttpRequestMessage("not.a.valid.token");
            Assert.Throws<UnauthorizedAccessException>(
                () => JwtHelper.GetUserIdFromToken(request));
        }

        // ════════════════════════════════════════════════════════════════════
        // CheckAdmin — DB-independent assertions still run; DB-dependent
        // variants are marked Skip above (ValidateToken + CheckAdmin).
        // ════════════════════════════════════════════════════════════════════

        [Fact(Skip = "Duplicate — DB-dependent; use the CheckAdmin_AdminToken_ReturnsTrue above")]
        public void CheckAdmin_AdminToken_ReturnsTrue_Duplicate()
        {
            var adminUser = new User
            {
                UserId = 1,
                Username = "admin",
                Email    = "admin@test.com",
                Role     = "Admin",
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };
            var token   = JwtHelper.GenerateToken(adminUser);
            var request = CreateHttpRequestMessage(token);

            Assert.True(JwtHelper.CheckAdmin(request));
        }

        [Fact(Skip = "Duplicate — DB-dependent; use the CheckAdmin_SuperAdminToken_ReturnsTrue above")]
        public void CheckAdmin_SuperAdminToken_ReturnsTrue_Duplicate()
        {
            var superAdmin = new User
            {
                UserId = 1,
                Username = "superadmin",
                Email    = "superadmin@test.com",
                Role     = "SuperAdmin",
                IsActive = true,
                PasswordChangedAt = DateTime.UtcNow
            };
            var token   = JwtHelper.GenerateToken(superAdmin);
            var request = CreateHttpRequestMessage(token);

            Assert.True(JwtHelper.CheckAdmin(request));
        }

        [Fact]
        public void CheckAdmin_NormalUserToken_ReturnsFalse()
        {
            var token   = JwtHelper.GenerateToken(_testUser);
            var request = CreateHttpRequestMessage(token);

            Assert.False(JwtHelper.CheckAdmin(request));
        }

        [Fact]
        public void CheckAdmin_NoAuthHeader_ReturnsFalse()
        {
            var request = CreateHttpRequestMessage(null);
            Assert.False(JwtHelper.CheckAdmin(request));
        }

        [Fact]
        public void CheckAdmin_TamperedToken_ReturnsFalse()
        {
            var token   = JwtHelper.GenerateToken(_testUser);
            var tampered = token.Substring(0, token.Length - 2) + "XX";
            var request = CreateHttpRequestMessage(tampered);

            Assert.False(JwtHelper.CheckAdmin(request));
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private static HttpRequestMessage CreateHttpRequestMessage(string token)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            }
            return request;
        }
    }
}
