using FluentAssertions;
using GiveAID.Application.Features.Auth.Commands.Login;

namespace GiveAID.Tests.Unit.Application.Validators;

public class LoginCommandValidatorTests
{
    private readonly LoginCommandValidator _validator;

    public LoginCommandValidatorTests()
    {
        _validator = new LoginCommandValidator();
    }

    [Fact]
    public void ValidCommand_PassesValidation()
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = "password123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("ab")]        // too short
    [InlineData("a@b")]       // contains @
    [InlineData("user name")] // contains space
    [InlineData("user!")]     // contains !
    public void InvalidUsername_FailsValidation(string? username)
    {
        var command = new LoginCommand
        {
            Username = username!,
            Password = "password123"
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("testuser")]
    [InlineData("test_user")]
    [InlineData("test.user")]
    [InlineData("test-user")]
    [InlineData("User123")]
    public void ValidUsername_PassesValidation(string username)
    {
        var command = new LoginCommand
        {
            Username = username,
            Password = "password123"
        };

        var result = _validator.Validate(command);

        result.Errors.Should().NotContain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("short")] // less than 6 chars
    public void InvalidPassword_FailsValidation(string? password)
    {
        var command = new LoginCommand
        {
            Username = "testuser",
            Password = password!
        };

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password");
    }
}
