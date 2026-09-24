using FluentAssertions;
using GiveAID.Infrastructure.Security;

namespace GiveAID.Tests.Unit.Infrastructure.Security;

public class PasswordHasherTests
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    public void Hash_ProducesNonEmptyString()
    {
        // Act
        var hash = _hasher.Hash("password123");

        // Assert
        hash.Should().NotBeNullOrEmpty();
        hash.Should().NotBe("password123");
    }

    [Fact]
    public void Hash_DoesNotEqualPlainText()
    {
        // Act
        var hash = _hasher.Hash("MySecretPassword");

        // Assert
        hash.Should().NotBe("MySecretPassword");
        hash.Length.Should().BeGreaterThan(20); // BCrypt hashes are typically 60 chars
    }

    [Fact]
    public void Verify_CorrectPassword_ReturnsTrue()
    {
        // Arrange
        var password = "password123";
        var hash = _hasher.Hash(password);

        // Act
        var result = _hasher.Verify(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _hasher.Hash("correctPassword");

        // Act
        var result = _hasher.Verify("wrongPassword", hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_SamePasswordTwice_ProducesDifferentHashes()
    {
        // Act
        var hash1 = _hasher.Hash("password123");
        var hash2 = _hasher.Hash("password123");

        // Assert
        hash1.Should().NotBe(hash2); // Due to random salt
    }

    [Fact]
    public void Verify_BothHashesOfSamePassword_BothReturnTrue()
    {
        // Arrange
        var password = "password123";
        var hash1 = _hasher.Hash(password);
        var hash2 = _hasher.Hash(password);

        // Act
        var result1 = _hasher.Verify(password, hash1);
        var result2 = _hasher.Verify(password, hash2);

        // Assert
        result1.Should().BeTrue();
        result2.Should().BeTrue();
    }

    [Fact]
    public void Hash_EmptyString_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _hasher.Hash(string.Empty);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Hash_NullString_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _hasher.Hash(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Verify_EmptyPassword_ReturnsFalse()
    {
        // Arrange
        var hash = _hasher.Hash("password123");

        // Act
        var result = _hasher.Verify(string.Empty, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Verify_InvalidHash_ReturnsFalse()
    {
        // Act
        var result = _hasher.Verify("password", "invalidhash");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Hash_LongPassword_ProducesValidHash()
    {
        // Arrange
        var longPassword = new string('a', 1000);

        // Act
        var hash = _hasher.Hash(longPassword);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        _hasher.Verify(longPassword, hash).Should().BeTrue();
    }

    [Fact]
    public void Hash_UnicodePassword_ProducesValidHash()
    {
        // Arrange
        var unicodePassword = "пароль密码🔐";

        // Act
        var hash = _hasher.Hash(unicodePassword);

        // Assert
        hash.Should().NotBeNullOrEmpty();
        _hasher.Verify(unicodePassword, hash).Should().BeTrue();
    }
}
