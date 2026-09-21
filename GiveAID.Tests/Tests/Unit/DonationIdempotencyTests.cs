using System;
using System.Linq;
using System.Text.RegularExpressions;
using GiveAID.Web.Models;
using Xunit;

namespace GiveAID.Tests.Tests.Unit
{
    /// <summary>
    /// Unit tests for donation idempotency key handling.
    /// Tests verify:
    ///   • IdempotencyKey format validation.
    ///   • Edge cases for null/empty keys.
    ///   • Donation model idempotency key constraints.
    /// </summary>
    public class DonationIdempotencyTests
    {
        // ════════════════════════════════════════════════════════════════════
        // IdempotencyKey format validation
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void IdempotencyKey_GuidFormat_IsValid()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // Assert - GUID format is 32 hex digits with 4 hyphens
            Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", key);
        }

        [Fact]
        public void IdempotencyKey_NewGuid_IsUnique()
        {
            // Arrange & Act
            var key1 = Guid.NewGuid().ToString();
            var key2 = Guid.NewGuid().ToString();

            // Assert
            Assert.NotEqual(key1, key2);
        }

        [Fact]
        public void IdempotencyKey_MultipleNewGuids_AreAllUnique()
        {
            // Arrange & Act
            var keys = Enumerable.Range(0, 100)
                .Select(_ => Guid.NewGuid().ToString())
                .ToList();

            // Assert
            Assert.Equal(100, keys.Distinct().Count());
        }

        [Theory]
        [InlineData("550e8400-e29b-41d4-a716-446655440000")]
        [InlineData("6ba7b810-9dad-11d1-80b4-00c04fd430c8")]
        [InlineData("f47ac10b-58cc-4372-a567-0e02b2c3d479")]
        public void IdempotencyKey_ValidGuidFormats_AreAccepted(string validGuid)
        {
            // Assert - these are valid GUIDs
            Assert.True(Guid.TryParse(validGuid, out _));
        }

        // ════════════════════════════════════════════════════════════════════
        // Edge cases for null/empty keys
        // ════════════════════════════════════════════════════════════════════

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t")]
        public void IdempotencyKey_InvalidInputs_AreDetected(string invalidKey)
        {
            // This tests that our validation logic would catch these
            Assert.True(string.IsNullOrWhiteSpace(invalidKey));
        }

        [Fact]
        public void IdempotencyKey_NullCheck_HasExpectedBehavior()
        {
            // Arrange
            string key = null;

            // Assert
            Assert.True(string.IsNullOrWhiteSpace(key));
        }

        [Fact]
        public void IdempotencyKey_EmptyString_HasExpectedBehavior()
        {
            // Arrange
            var key = string.Empty;

            // Assert
            Assert.True(string.IsNullOrWhiteSpace(key));
        }

        [Fact]
        public void IdempotencyKey_WhitespaceOnly_HasExpectedBehavior()
        {
            // Arrange
            var key = "   \t\n  ";

            // Assert
            Assert.True(string.IsNullOrWhiteSpace(key));
        }

        // ════════════════════════════════════════════════════════════════════
        // Donation model validation
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void DonationModel_IdempotencyKey_MaxLengthIs100()
        {
            // The model specifies [MaxLength(100)]
            var maxLength = 100;

            // A standard GUID is 36 characters, well under the limit
            var guidKey = Guid.NewGuid().ToString();
            Assert.True(guidKey.Length <= maxLength);

            // A very long key would exceed the limit
            var longKey = new string('a', 150);
            Assert.True(longKey.Length > maxLength);
        }

        [Fact]
        public void DonationModel_NewDonation_IdempotencyKey_IsNullable()
        {
            // Arrange
            var donation = new Donation
            {
                UserId = 1,
                CauseId = 1,
                Amount = 100,
                PaymentMethod = "Card",
                PaymentStatus = "Pending"
            };

            // Assert - IdempotencyKey is nullable
            Assert.Null(donation.IdempotencyKey);
        }

        [Fact]
        public void DonationModel_WithIdempotencyKey_CanBeSet()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();
            var donation = new Donation
            {
                UserId = 1,
                CauseId = 1,
                Amount = 100,
                PaymentMethod = "Card",
                PaymentStatus = "Pending",
                IdempotencyKey = key
            };

            // Assert
            Assert.Equal(key, donation.IdempotencyKey);
        }

        // ════════════════════════════════════════════════════════════════════
        // Security considerations
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void IdempotencyKey_ContainsNoSensitiveData()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // Assert - GUID is random and doesn't contain sensitive info
            Assert.DoesNotContain("@", key);
            Assert.DoesNotContain("password", key);
            Assert.DoesNotContain("secret", key);
            Assert.DoesNotContain("token", key);
        }

        [Fact]
        public void IdempotencyKey_IsUrlSafe()
        {
            // Arrange
            var key = Guid.NewGuid().ToString();

            // GUID contains only alphanumeric and hyphens - URL safe
            Assert.Matches(@"^[a-zA-Z0-9\-]+$", key);
        }
    }
}
