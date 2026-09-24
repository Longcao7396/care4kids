using FluentAssertions;
using GiveAID.Infrastructure.Security;
using GiveAID.Infrastructure.Auth;
using Microsoft.Extensions.Options;

namespace GiveAID.Tests.Unit.Infrastructure.Security;

public class JwtTokenServiceTests
{
    private readonly IOptions<JwtSettings> _jwtOptions;

    public JwtTokenServiceTests()
    {
        var settings = new JwtSettings
        {
            Secret = "TestSecretKeyForJwtTokenGenerationMustBeAtLeast64BytesLong-2024",
            Issuer = "GiveAID.Test",
            Audience = "GiveAID.Test.Client",
            ExpiryMinutes = 60
        };
        
        _jwtOptions = Options.Create(settings);
    }

    [Fact]
    public void GetTokenExpiration_ReturnsFutureDate()
    {
        // Arrange
        // Using a mock DbContext that returns empty list
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);

        // Act
        var beforeCall = DateTime.UtcNow;
        var expiresAt = service.GetTokenExpiration();
        var afterCall = DateTime.UtcNow;

        // Assert
        expiresAt.Should().BeAfter(beforeCall.AddMinutes(59));
        expiresAt.Should().BeBefore(afterCall.AddMinutes(61));
    }

    [Fact]
    public void ValidateToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);

        // Act
        var userId = service.ValidateToken("invalid.token.here");

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void ValidateToken_EmptyToken_ReturnsNull()
    {
        // Arrange
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);

        // Act
        var userId = service.ValidateToken(string.Empty);

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GetUserIdFromToken_ValidToken_ReturnsUserId()
    {
        // Arrange
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);
        
        // Generate a valid token first
        var token = service.GenerateToken(42, "test@example.com", "User");

        // Act
        var userId = service.GetUserIdFromToken(token);

        // Assert
        userId.Should().Be(42);
    }

    [Fact]
    public void GetUserIdFromToken_InvalidToken_ReturnsNull()
    {
        // Arrange
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);

        // Act
        var userId = service.GetUserIdFromToken("invalid.token");

        // Assert
        userId.Should().BeNull();
    }

    [Fact]
    public void GenerateToken_ProducesValidJwtFormat()
    {
        // Arrange
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);

        // Act
        var token = service.GenerateToken(1, "test@example.com", "User");

        // Assert
        token.Should().NotBeNullOrEmpty();
        token.Split('.').Should().HaveCount(3); // JWT format: header.payload.signature
    }

    [Fact]
    public void GenerateToken_DifferentUsers_ProducesDifferentTokens()
    {
        // Arrange
        var mockDbContext = TestDbContextFactory.CreateMock();
        var service = new JwtTokenService(_jwtOptions, mockDbContext);

        // Act
        var token1 = service.GenerateToken(1, "user1@example.com", "User");
        var token2 = service.GenerateToken(2, "user2@example.com", "User");

        // Assert - Tokens should be different due to unique claims (jti, iat)
        token1.Should().NotBe(token2);
    }
}
