using FluentValidation;
using GiveAID.Application.Features.Campaigns.Commands.Create;

namespace GiveAID.Application.Features.Campaigns.Validators;

/// <summary>
/// Validator for CreateCampaignCommand.
/// L-05: Validates campaign dates to prevent invalid date ranges.
/// </summary>
public class CreateCampaignCommandValidator : AbstractValidator<CreateCampaignCommand>
{
    public CreateCampaignCommandValidator()
    {
        RuleFor(x => x.CampaignName)
            .NotEmpty().WithMessage("Campaign name is required.")
            .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters.");

        RuleFor(x => x.CauseId)
            .GreaterThan(0).WithMessage("Valid cause is required.");

        RuleFor(x => x.GoalAmount)
            .GreaterThan(0).WithMessage("Goal amount must be greater than zero.");

        // L-05: EndDate must be after StartDate
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.")
            .When(x => x.EndDate.HasValue);

        // L-05: EndDate must be in the future (if set)
        RuleFor(x => x.EndDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("End date must be in the future.")
            .When(x => x.EndDate.HasValue);

        // L-05: StartDate should not be too far in the past (within last 30 days)
        RuleFor(x => x.StartDate)
            .GreaterThan(DateTime.UtcNow.AddDays(-30))
            .WithMessage("Start date cannot be more than 30 days in the past.")
            .When(x => x.StartDate != default);
    }
}
