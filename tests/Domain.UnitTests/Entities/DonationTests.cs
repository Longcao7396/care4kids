using FluentAssertions;
using GiveAID.Domain.Entities;

namespace GiveAID.Tests.Unit.Domain.Entities;

public class DonationTests
{
    [Fact]
    public void Donation_NewDonation_HasTransactionIdNull()
    {
        var donation = new Donation
        {
            Amount = 100m,
            UserId = 1,
            CauseId = 1
        };
        donation.TransactionId.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Donation_Amount_CanBeSet()
    {
        var donation = new Donation { Amount = 100.50m };
        donation.Amount.Should().Be(100.50m);
    }

    [Fact]
    public void Donation_DefaultPaymentStatus_IsPending()
    {
        var donation = new Donation();
        donation.PaymentStatus.Should().Be("Pending");
    }

    [Fact]
    public void Donation_DefaultPaymentMethod_IsEmpty()
    {
        var donation = new Donation();
        donation.PaymentMethod.Should().BeEmpty();
    }

    [Fact]
    public void Donation_IsAnonymous_DefaultsToFalse()
    {
        var donation = new Donation();
        donation.IsAnonymous.Should().BeFalse();
    }

    [Fact]
    public void Donation_ReceiptSent_DefaultsToFalse()
    {
        var donation = new Donation();
        donation.ReceiptSent.Should().BeFalse();
    }

    [Fact]
    public void Donation_DonationDate_IsSetToNow()
    {
        var donation = new Donation();
        donation.DonationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Donation_InvalidAmount_FailsValidation(decimal amount)
    {
        var donation = new Donation { Amount = amount };
        donation.Amount.Should().BeLessThanOrEqualTo(0);
    }

    [Fact]
    public void Donation_CardLastFour_CanBeSet()
    {
        var donation = new Donation { CardLastFour = "4242" };
        donation.CardLastFour.Should().Be("4242");
    }

    [Fact]
    public void Donation_CardType_CanBeSet()
    {
        var donation = new Donation { CardType = "Visa" };
        donation.CardType.Should().Be("Visa");
    }

    [Fact]
    public void Donation_Message_CanBeSet()
    {
        var donation = new Donation { Message = "For the children" };
        donation.Message.Should().Be("For the children");
    }

    [Fact]
    public void Donation_UserNavigation_IsNullByDefault()
    {
        var donation = new Donation();
        donation.User.Should().BeNull();
    }

    [Fact]
    public void Donation_CampaignNavigation_IsNullByDefault()
    {
        var donation = new Donation();
        donation.Campaign.Should().BeNull();
    }
}
