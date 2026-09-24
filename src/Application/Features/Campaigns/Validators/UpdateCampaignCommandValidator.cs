using FluentValidation;
using GiveAID.Application.Features.Campaigns.Commands.Update;

namespace GiveAID.Application.Features.Campaigns.Validators;

/// <summary>
/// Validator for UpdateCampaignCommand.
/// L-05: Validates campaign dates to prevent invalid date ranges.
/// </summary>
public class UpdateCampaignCommandValidator : AbstractValidator<UpdateCampaignCommand>
{
    public UpdateCampaignCommandValidator()
    {
        RuleFor(x => x.CampaignId)
            .GreaterThan(0).WithMessage("Valid campaign ID is required.");

        RuleFor(x => x.CampaignName)
            .MaximumLength(200).WithMessage("Campaign name cannot exceed 200 characters.")
            .When(x => !string.IsNullOrEmpty(x.CampaignName));

        RuleFor(x => x.CauseId)
            .GreaterThan(0).WithMessage("Valid cause is required.")
            .When(x => x.CauseId.HasValue);

        RuleFor(x => x.GoalAmount)
            .GreaterThan(0).WithMessage("Goal amount must be greater than zero.")
            .When(x => x.GoalAmount.HasValue);

        // L-05: EndDate must be after StartDate (when both are provided)
        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.")
            .When(x => x.EndDate.HasValue && x.StartDate.HasValue);

        // L-05: EndDate must be in the future (if set and campaign is active)
        RuleFor(x => x.EndDate)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("End date must be in the future.")
            .When(x => x.EndDate.HasValue && x.Status == "Active");

        // L-05: Cannot set StartDate to the past if campaign is Active
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
            .WithMessage("Start date cannot be set too far in the future.")
            .When(x => x.StartDate.HasValue && x.Status == "Active");
    }
}
