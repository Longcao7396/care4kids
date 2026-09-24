using FluentAssertions;
using GiveAID.Application.Features.Auth.Commands.Login;
using GiveAID.Application.Features.Auth.DTOs;
using GiveAID.Application.Services;
using GiveAID.Application.Common.Interfaces;
using GiveAID.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using Moq;

namespace GiveAID.Tests.Unit.Application.Auth;

public class LoginHandlerTests
{
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IJwtTokenService> _jwtTokenServiceMock;
    private readonly Mock<IPasswordHasher> _passwordHasherMock;

    public LoginHandlerTests()
    {
        _contextMock = new Mock<IApplicationDbContext>();
        _jwtTokenServiceMock = new Mock<IJwtTokenService>();
        _passwordHasherMock = new Mock<IPasswordHasher>();
    }

    [Fact]
    public async Task Handle_WithValidUsername_ReturnsToken()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashed_pwd",
            Role = "User",
            IsActive = true,
            IsVerified = true
        };

        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User> { user }.AsQueryable()).Object);
        _passwordHasherMock.Setup(h => h.Verify("password123", "hashed_pwd")).Returns(true);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(1, "test@example.com", "User", "testuser"))
            .Returns("fake-jwt-token");
        _jwtTokenServiceMock.Setup(j => j.GetTokenExpiration()).Returns(DateTime.UtcNow.AddHours(1));

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.Email.Should().Be("test@example.com");
        result.Username.Should().Be("testuser");
        result.UserId.Should().Be(1);
        result.Role.Should().Be("User");
    }

    [Fact]
    public async Task Handle_WithMixedCaseUsername_ReturnsToken()
    {
        // Arrange — username lookup is case-insensitive
        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            Username = "TestUser",
            PasswordHash = "hashed_pwd",
            Role = "User",
            IsActive = true,
            IsVerified = true
        };

        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User> { user }.AsQueryable()).Object);
        _passwordHasherMock.Setup(h => h.Verify("password123", "hashed_pwd")).Returns(true);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(1, "test@example.com", "User", "TestUser"))
            .Returns("fake-jwt-token");
        _jwtTokenServiceMock.Setup(j => j.GetTokenExpiration()).Returns(DateTime.UtcNow.AddHours(1));

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "TESTUSER", Password = "password123" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("fake-jwt-token");
        result.Username.Should().Be("TestUser");
    }

    [Fact]
    public async Task Handle_WithUnknownUsername_ThrowsUnauthorized()
    {
        // Arrange — empty list means no user found
        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User>().AsQueryable()).Object);

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "nonexistent", Password = "pwd" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_WithEmailInsteadOfUsername_ThrowsUnauthorized()
    {
        // Email is no longer accepted as a login identifier.
        // Even if a user with that email exists, login by email must fail.
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_pwd",
            Role = "User",
            IsActive = true,
            IsVerified = true
        };

        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User> { user }.AsQueryable()).Object);

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "test@example.com", Password = "pwd" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_WithInvalidPassword_ThrowsUnauthorized()
    {
        // Arrange
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_pwd",
            Role = "User",
            IsActive = true,
            IsVerified = true
        };

        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User> { user }.AsQueryable()).Object);
        _passwordHasherMock.Setup(h => h.Verify("wrongpassword", "hashed_pwd")).Returns(false);

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "testuser", Password = "wrongpassword" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid username or password.");
    }

    [Fact]
    public async Task Handle_WithInactiveUser_ThrowsUnauthorized()
    {
        // Arrange — inactive user should not be returned by the query
        var user = new User
        {
            UserId = 1,
            Username = "testuser",
            Email = "test@example.com",
            PasswordHash = "hashed_pwd",
            Role = "User",
            IsActive = false
        };

        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User>().AsQueryable()).Object);

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_WithEmptyUsername_ThrowsUnauthorized()
    {
        // Arrange — empty list means no user found
        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User>().AsQueryable()).Object);

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "", Password = "password123" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Username is required.");
    }

    [Fact]
    public async Task Handle_WithWhitespaceUsername_ThrowsUnauthorized()
    {
        // Arrange — empty list means no user found
        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User>().AsQueryable()).Object);

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "   ", Password = "password123" };

        // Act & Assert
        await FluentActions.Invoking(() => handler.Handle(command, CancellationToken.None))
            .Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Username is required.");
    }

    [Fact]
    public async Task Handle_UpdatesLastLogin()
    {
        // Arrange
        var originalLastLogin = DateTime.UtcNow.AddDays(-1);
        var user = new User
        {
            UserId = 1,
            Email = "test@example.com",
            Username = "testuser",
            PasswordHash = "hashed_pwd",
            Role = "User",
            IsActive = true,
            IsVerified = true,
            LastLogin = originalLastLogin
        };

        _contextMock.Setup(c => c.Users)
            .Returns(DbSetMockHelper.CreateMockDbSet(new List<User> { user }.AsQueryable()).Object);
        _passwordHasherMock.Setup(h => h.Verify("password123", "hashed_pwd")).Returns(true);
        _jwtTokenServiceMock.Setup(j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("fake-jwt-token");
        _jwtTokenServiceMock.Setup(j => j.GetTokenExpiration()).Returns(DateTime.UtcNow.AddHours(1));

        var handler = new LoginCommandHandler(_contextMock.Object, _jwtTokenServiceMock.Object, _passwordHasherMock.Object);
        var command = new LoginCommand { Username = "testuser", Password = "password123" };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        user.LastLogin.Should().BeAfter(originalLastLogin);
    }
}

public static class DbSetMockHelper
{
    public static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> data) where T : class
    {
        return data.ToList().BuildMockDbSet();
    }
}
