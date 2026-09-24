using FluentAssertions;
using GiveAID.Domain.Entities;

namespace GiveAID.Tests.Unit.Domain.Entities;

public class UserTests
{
    [Fact]
    public void User_NewUser_HasCreatedAtSet()
    {
        var user = new User();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void User_DefaultRole_IsUser()
    {
        var user = new User();
        user.Role.Should().Be("User");
    }

    [Fact]
    public void User_IsActive_DefaultsToTrue()
    {
        var user = new User();
        user.IsActive.Should().BeTrue();
    }

    [Fact]
    public void User_IsVerified_DefaultsToFalse()
    {
        var user = new User();
        user.IsVerified.Should().BeFalse();
    }

    [Fact]
    public void User_Username_CanBeSet()
    {
        var user = new User { Username = "testuser" };
        user.Username.Should().Be("testuser");
    }

    [Fact]
    public void User_Email_CanBeSet()
    {
        var user = new User { Email = "test@example.com" };
        user.Email.Should().Be("test@example.com");
    }

    [Fact]
    public void User_FullName_CanBeSet()
    {
        var user = new User { FullName = "Test User" };
        user.FullName.Should().Be("Test User");
    }

    [Fact]
    public void User_PasswordHash_CanBeSet()
    {
        var user = new User { PasswordHash = "hashed_value" };
        user.PasswordHash.Should().Be("hashed_value");
    }

    [Fact]
    public void User_Donations_IsInitializedAsEmptyList()
    {
        var user = new User();
        user.Donations.Should().NotBeNull();
        user.Donations.Should().BeEmpty();
    }

    [Fact]
    public void User_CampaignRegistrations_IsInitializedAsEmptyList()
    {
        var user = new User();
        user.CampaignRegistrations.Should().NotBeNull();
        user.CampaignRegistrations.Should().BeEmpty();
    }

    [Fact]
    public void User_Phone_CanBeSet()
    {
        var user = new User { Phone = "1234567890" };
        user.Phone.Should().Be("1234567890");
    }

    [Fact]
    public void User_Address_CanBeSet()
    {
        var user = new User { Address = "123 Main St" };
        user.Address.Should().Be("123 Main St");
    }

    [Fact]
    public void User_Profession_CanBeSet()
    {
        var user = new User { Profession = "Developer" };
        user.Profession.Should().Be("Developer");
    }

    [Fact]
    public void User_VerificationToken_CanBeSet()
    {
        var user = new User { VerificationToken = "token123" };
        user.VerificationToken.Should().Be("token123");
    }

    [Fact]
    public void User_AdminRole_CanBeSet()
    {
        var user = new User { Role = "Admin" };
        user.Role.Should().Be("Admin");
    }

    [Fact]
    public void User_LastLogin_CanBeSet()
    {
        var lastLogin = DateTime.UtcNow.AddHours(-1);
        var user = new User { LastLogin = lastLogin };
        user.LastLogin.Should().Be(lastLogin);
    }

    [Fact]
    public void User_Gender_CanBeSet()
    {
        var user = new User { Gender = "Male" };
        user.Gender.Should().Be("Male");
    }
}
