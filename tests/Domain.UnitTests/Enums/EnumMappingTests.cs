using FluentAssertions;
using GiveAID.Domain.Enums;

namespace GiveAID.Tests.Unit.Domain.Enums;

public class CampaignStatusTests
{
    [Fact]
    public void CampaignStatus_Draft_HasCorrectValue()
    {
        ((int)CampaignStatus.Draft).Should().Be(1);
    }

    [Fact]
    public void CampaignStatus_Active_HasCorrectValue()
    {
        ((int)CampaignStatus.Active).Should().Be(2);
    }

    [Fact]
    public void CampaignStatus_Completed_HasCorrectValue()
    {
        ((int)CampaignStatus.Completed).Should().Be(3);
    }

    [Fact]
    public void CampaignStatus_Cancelled_HasCorrectValue()
    {
        ((int)CampaignStatus.Cancelled).Should().Be(4);
    }

    [Fact]
    public void CampaignStatus_AllStatuses_HaveUniqueValues()
    {
        var values = Enum.GetValues<CampaignStatus>();
        values.Should().HaveCount(4);
    }
}

public class PaymentStatusTests
{
    [Fact]
    public void PaymentStatus_Pending_HasCorrectValue()
    {
        ((int)PaymentStatus.Pending).Should().Be(1);
    }

    [Fact]
    public void PaymentStatus_Completed_HasCorrectValue()
    {
        ((int)PaymentStatus.Completed).Should().Be(2);
    }

    [Fact]
    public void PaymentStatus_Failed_HasCorrectValue()
    {
        ((int)PaymentStatus.Failed).Should().Be(3);
    }

    [Fact]
    public void PaymentStatus_Refunded_HasCorrectValue()
    {
        ((int)PaymentStatus.Refunded).Should().Be(4);
    }
}

public class UserRoleTests
{
    [Fact]
    public void UserRole_User_HasCorrectValue()
    {
        ((int)UserRole.User).Should().Be(3);
    }

    [Fact]
    public void UserRole_Admin_HasCorrectValue()
    {
        ((int)UserRole.Admin).Should().Be(1);
    }

    [Fact]
    public void UserRole_ContentManager_HasCorrectValue()
    {
        ((int)UserRole.ContentManager).Should().Be(2);
    }
}
