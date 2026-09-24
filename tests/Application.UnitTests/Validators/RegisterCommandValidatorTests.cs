using FluentAssertions;
using GiveAID.Application.Features.Auth.Commands.Register;

namespace GiveAID.Tests.Unit.Application.Validators;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyUsername_FailsValidation(string? username)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = username!,
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("ab")] // Less than 3 characters
    public void ShortUsername_FailsValidation(string username)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = username,
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyEmail_FailsValidation(string? email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = email!,
            Password = "Password123!",
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("test@")]
    [InlineData("test")]
    public void InvalidEmailFormat_FailsValidation(string email)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = email,
            Password = "Password123!",
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyPassword_FailsValidation(string? password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = password!,
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }

    [Theory]
    [InlineData("short")]           // Less than 8 characters
    [InlineData("alllowercase1")]    // No uppercase
    [InlineData("ALLUPPERCASE1")]    // No lowercase
    [InlineData("NoNumbersHere")]    // No number
    [InlineData("Pass1234!")]         // Valid
    public void PasswordStrength_ValidatesCorrectly(string password)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = password,
            FullName = "Test User"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        if (password == "Pass1234!")
        {
            result.Errors.Should().NotContain(e => e.PropertyName == "Password");
        }
        else
        {
            result.Errors.Should().Contain(e => e.PropertyName == "Password");
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void EmptyFullName_FailsValidation(string? fullName)
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = fullName!
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "FullName");
    }

    [Fact]
    public void ValidPhone_PassesValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "Test User",
            Phone = "1234567890"
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Phone");
    }

    [Fact]
    public void NullPhone_PassesValidation()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Password123!",
            FullName = "Test User",
            Phone = null
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Phone");
    }
}
