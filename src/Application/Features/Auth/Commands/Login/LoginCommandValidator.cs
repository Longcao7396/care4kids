using FluentValidation;
using MediatR;

namespace GiveAID.Application.Features.Auth.Commands.Login;

/// <summary>
/// Validator for LoginCommand. Username is required (email is no longer
/// accepted as a login identifier).
/// </summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .Length(3, 64).WithMessage("Username must be between 3 and 64 characters.")
            .Matches("^[A-Za-z0-9_.-]+$")
                .WithMessage("Username may only contain letters, digits, underscore, dot, or hyphen.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.");
    }
}
