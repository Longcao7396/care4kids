using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using GiveAID.Application.Features.Donations.Commands.Create;

namespace GiveAID.Tests.Unit.Application.Donations;

/// <summary>
/// H-01: Server-side donation amount validation tests.
/// Ensures that amounts <= 0 are rejected even via direct API call.
/// </summary>
public class CreateDonationCommandValidatorTests
{
    private readonly CreateDonationCommandValidator _validator;

    public CreateDonationCommandValidatorTests()
    {
        _validator = new CreateDonationCommandValidator();
    }

    [Theory]
    [InlineData(1.00)]
    [InlineData(100.0)]
    [InlineData(999999.99)]
    public async Task Validate_PositiveAmount_Passes(decimal amount)
    {
        // Arrange
        var command = new CreateDonationCommand { Amount = amount, CauseId = 1 };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    [InlineData(-0.01)]
    public async Task Validate_ZeroOrNegativeAmount_Fails(decimal amount)
    {
        // Arrange
        var command = new CreateDonationCommand { Amount = amount, CauseId = 1 };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateDonationCommand.Amount));
    }

    [Fact]
    public async Task Validate_ZeroAmount_ErrorMessage_IsDescriptive()
    {
        // Arrange
        var command = new CreateDonationCommand { Amount = 0, CauseId = 1 };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Should().Contain(e =>
            e.ErrorMessage.Contains("zero", StringComparison.OrdinalIgnoreCase) ||
            e.ErrorMessage.Contains("greater than", StringComparison.OrdinalIgnoreCase));
    }
}
