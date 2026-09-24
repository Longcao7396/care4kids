using FluentValidation;

namespace GiveAID.Application.Features.Donations.Commands.Create;

/// <summary>
/// Validator for CreateDonationCommand.
/// Ensures server-side rejection of invalid donation amounts.
/// </summary>
public class CreateDonationCommandValidator : AbstractValidator<CreateDonationCommand>
{
    public CreateDonationCommandValidator()
    {
        // M-07 & M-08: Donation amount validation - must be between 1.00 and 1,000,000.00
        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Donation amount must be greater than zero.")
            .GreaterThanOrEqualTo(1.00m)
            .WithMessage("Minimum donation amount is 1.00.")
            .LessThanOrEqualTo(1000000.00m)
            .WithMessage("Maximum donation amount is 1,000,000.00.");

        // M-02: CampaignId must be greater than 0 when provided (null is allowed for general donations)
        RuleFor(x => x.CampaignId)
            .GreaterThan(0).When(x => x.CampaignId.HasValue)
            .WithMessage("CampaignId must be a valid campaign.");

        // M-03: Anonymous donation requires valid email
        RuleFor(x => x.Email)
            .NotEmpty().When(x => x.UserId == null)
            .WithMessage("Valid email is required for anonymous donations.")
            .EmailAddress().When(x => !string.IsNullOrEmpty(x.Email))
            .WithMessage("Please provide a valid email address.");
    }
}
