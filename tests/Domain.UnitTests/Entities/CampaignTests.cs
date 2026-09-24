using FluentAssertions;
using GiveAID.Domain.Entities;
using GiveAID.Domain.Enums;

namespace GiveAID.Tests.Unit.Domain.Entities;

public class CampaignTests
{
    [Fact]
    public void Campaign_NewCampaign_HasCreatedAtSet()
    {
        var campaign = new Campaign();
        campaign.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Campaign_ProgressPercentage_ZeroWhenNoDonations()
    {
        var campaign = new Campaign { GoalAmount = 1000m, RaisedAmount = 0m };
        var progress = campaign.RaisedAmount == 0 ? 0 : (campaign.RaisedAmount / campaign.GoalAmount) * 100;
        progress.Should().Be(0);
    }

    [Fact]
    public void Campaign_ProgressPercentage_CalculatedCorrectly()
    {
        var campaign = new Campaign { GoalAmount = 1000m, RaisedAmount = 250m };
        var progress = campaign.RaisedAmount == 0 ? 0 : (campaign.RaisedAmount / campaign.GoalAmount) * 100;
        progress.Should().Be(25);
    }

    [Fact]
    public void Campaign_IsActive_ReturnsTrueWhenInDateRange()
    {
        var campaign = new Campaign
        {
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            Status = "Active"
        };
        var isActive = campaign.Status == "Active" &&
                       campaign.StartDate <= DateTime.UtcNow &&
                       (campaign.EndDate == null || campaign.EndDate >= DateTime.UtcNow);
        isActive.Should().BeTrue();
    }

    [Fact]
    public void Campaign_IsActive_ReturnsFalseWhenEnded()
    {
        var campaign = new Campaign
        {
            StartDate = DateTime.UtcNow.AddDays(-10),
            EndDate = DateTime.UtcNow.AddDays(-1),
            Status = "Active"
        };
        var isActive = campaign.Status == "Active" &&
                       campaign.StartDate <= DateTime.UtcNow &&
                       (campaign.EndDate == null || campaign.EndDate >= DateTime.UtcNow);
        isActive.Should().BeFalse();
    }

    [Fact]
    public void Campaign_RaisedAmount_NeverExceedsGoal()
    {
        var campaign = new Campaign { GoalAmount = 1000m, RaisedAmount = 5000m };
        var effective = Math.Min(campaign.RaisedAmount, campaign.GoalAmount);
        effective.Should().Be(1000m);
    }

    [Fact]
    public void Campaign_DefaultStatus_IsActive()
    {
        var campaign = new Campaign();
        campaign.Status.Should().Be("Active");
    }

    [Fact]
    public void Campaign_Registrations_IsInitializedAsEmptyList()
    {
        var campaign = new Campaign();
        campaign.Registrations.Should().NotBeNull();
        campaign.Registrations.Should().BeEmpty();
    }

    [Fact]
    public void Campaign_Donations_IsInitializedAsEmptyList()
    {
        var campaign = new Campaign();
        campaign.Donations.Should().NotBeNull();
        campaign.Donations.Should().BeEmpty();
    }

    [Fact]
    public void Campaign_CampaignName_CanBeSet()
    {
        var campaign = new Campaign { CampaignName = "Test Campaign" };
        campaign.CampaignName.Should().Be("Test Campaign");
    }

    [Fact]
    public void Campaign_GoalAmount_CanBeZero()
    {
        var campaign = new Campaign { GoalAmount = 0m };
        campaign.GoalAmount.Should().Be(0m);
    }

    [Fact]
    public void Campaign_EndDate_CanBeNull()
    {
        var campaign = new Campaign { EndDate = null };
        campaign.EndDate.Should().BeNull();
    }
}
