using FluentAssertions;
using GiveAID.Domain.Entities;

namespace GiveAID.Tests.Unit.Domain.Entities;

public class CauseTests
{
    [Fact]
    public void Cause_NewCause_HasCreatedAtSet()
    {
        var cause = new Cause();
        cause.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Cause_CauseName_CanBeSet()
    {
        var cause = new Cause { CauseName = "Education" };
        cause.CauseName.Should().Be("Education");
    }

    [Fact]
    public void Cause_Description_CanBeSet()
    {
        var cause = new Cause { Description = "Help children get education" };
        cause.Description.Should().Be("Help children get education");
    }

    [Fact]
    public void Cause_ImageUrl_CanBeSet()
    {
        var cause = new Cause { ImageUrl = "/images/education.jpg" };
        cause.ImageUrl.Should().Be("/images/education.jpg");
    }

    [Fact]
    public void Cause_Icon_CanBeSet()
    {
        var cause = new Cause { Icon = "book" };
        cause.Icon.Should().Be("book");
    }

    [Fact]
    public void Cause_TargetAmount_DefaultsToZero()
    {
        var cause = new Cause();
        cause.TargetAmount.Should().Be(0m);
    }

    [Fact]
    public void Cause_RaisedAmount_DefaultsToZero()
    {
        var cause = new Cause();
        cause.RaisedAmount.Should().Be(0m);
    }

    [Fact]
    public void Cause_IsActive_DefaultsToTrue()
    {
        var cause = new Cause();
        cause.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Cause_DisplayOrder_DefaultsToZero()
    {
        var cause = new Cause();
        cause.DisplayOrder.Should().Be(0);
    }

    [Fact]
    public void Cause_ParentCauseId_CanBeNull()
    {
        var cause = new Cause { ParentCauseId = null };
        cause.ParentCauseId.Should().BeNull();
    }

    [Fact]
    public void Cause_SubCauses_IsInitializedAsEmptyList()
    {
        var cause = new Cause();
        cause.SubCauses.Should().NotBeNull();
        cause.SubCauses.Should().BeEmpty();
    }

    [Fact]
    public void Cause_Campaigns_IsInitializedAsEmptyList()
    {
        var cause = new Cause();
        cause.Campaigns.Should().NotBeNull();
        cause.Campaigns.Should().BeEmpty();
    }

    [Fact]
    public void Cause_ProgressPercentage_CalculatedCorrectly()
    {
        var cause = new Cause { TargetAmount = 10000m, RaisedAmount = 2500m };
        var progress = cause.TargetAmount == 0 ? 0 : (cause.RaisedAmount / cause.TargetAmount) * 100;
        progress.Should().Be(25);
    }

    [Fact]
    public void Cause_ProgressPercentage_ZeroWhenNoTarget()
    {
        var cause = new Cause { TargetAmount = 0m, RaisedAmount = 1000m };
        var progress = cause.TargetAmount == 0 ? 0 : (cause.RaisedAmount / cause.TargetAmount) * 100;
        progress.Should().Be(0);
    }

    [Fact]
    public void Cause_CauseCode_CanBeSet()
    {
        var cause = new Cause { CauseCode = "EDU001" };
        cause.CauseCode.Should().Be("EDU001");
    }
}
