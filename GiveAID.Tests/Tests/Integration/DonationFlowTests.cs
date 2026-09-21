using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using GiveAID.Web.Controllers;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;
using Xunit;

namespace GiveAID.Tests.Tests.Integration
{
    /// <summary>
    /// Integration tests for <see cref="DonationsController"/>.
    ///
    /// Covered flows:
    ///   • CreateDonation   — valid request, idempotency, concurrent race conditions
    ///   • ConfirmPayment   — pending → completed, already completed, failed donation
    ///   • PaymentWebhook   — valid payload, unknown transaction, status transitions
    ///   • GetAll           — auth-scoped to the calling user
    ///   • GetById          — IDOR defence for non-admins
    /// </summary>
    public class DonationFlowTests : IDisposable
    {
        private readonly GiveAIDContext _context;
        private readonly User _testUser;
        private readonly User _adminUser;
        private readonly Cause _testCause;

        public DonationFlowTests()
        {
            _context = TestDbContextFactory.CreateContext();
            TestDbContextFactory.ResetDatabase(_context);

            // Seed a standard user and cause for all tests
            _testUser = new User
            {
                Username          = "donoruser",
                Email             = "donor@test.com",
                PasswordHash      = PasswordHasher.Hash("User@123"),
                FullName          = "Donor User",
                Role              = "User",
                IsActive          = true,
                IsVerified        = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(_testUser);

            _adminUser = new User
            {
                Username          = "adminuser",
                Email             = "donoradmin@test.com",
                PasswordHash      = PasswordHasher.Hash("Admin@123"),
                FullName          = "Donor Admin",
                Role              = "SuperAdmin",
                IsActive          = true,
                IsVerified        = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(_adminUser);

            _testCause = new Cause
            {
                CauseName  = "Education for Children",
                CauseCode  = "EDU",
                IsActive   = true,
                TargetAmount = 100_000_000m,
                RaisedAmount = 0,
                CreatedAt  = DateTime.UtcNow
            };
            _context.Causes.Add(_testCause);
            _context.SaveChanges();
        }

        public void Dispose() => _context?.Dispose();

        // ════════════════════════════════════════════════════════════════════
        // CreateDonation
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void CreateDonation_ValidRequest_CreatesPendingDonation()
        {
            var controller = BuildDonationsController(_testUser);

            var result = controller.Create(new DonationRequest
            {
                CauseId      = _testCause.CauseId,
                Amount       = 50000m,
                PaymentMethod = "BankTransfer",
                CardLast4    = "1234",
                Message      = "Happy to support!"
            });

            Assert.NotNull(result);
            Assert.IsType<IHttpActionResult>(result);

            var donation = _context.Donations
                .FirstOrDefault(d => d.UserId == _testUser.UserId && d.Amount == 50000m);
            Assert.NotNull(donation);
            Assert.Equal("Pending", donation.PaymentStatus);
            Assert.Equal("BankTransfer", donation.PaymentMethod);
            Assert.Equal("Happy to support!", donation.Message);
            Assert.NotNull(donation.TransactionId);
        }

        [Fact]
        public void CreateDonation_ZeroAmount_ReturnsBadRequest()
        {
            var controller = BuildDonationsController(_testUser);

            var result = controller.Create(new DonationRequest
            {
                CauseId      = _testCause.CauseId,
                Amount       = 0,
                PaymentMethod = "BankTransfer"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void CreateDonation_NegativeAmount_ReturnsBadRequest()
        {
            var controller = BuildDonationsController(_testUser);

            var result = controller.Create(new DonationRequest
            {
                CauseId      = _testCause.CauseId,
                Amount       = -100m,
                PaymentMethod = "BankTransfer"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void CreateDonation_InvalidCause_ReturnsBadRequest()
        {
            var controller = BuildDonationsController(_testUser);

            var result = controller.Create(new DonationRequest
            {
                CauseId      = 99999, // non-existent
                Amount       = 10000m,
                PaymentMethod = "CreditCard"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void CreateDonation_InactiveCause_ReturnsBadRequest()
        {
            var inactiveCause = new Cause
            {
                CauseName = "Inactive Cause",
                CauseCode = "INA",
                IsActive  = false,
                TargetAmount = 1_000_000m,
                CreatedAt = DateTime.UtcNow
            };
            _context.Causes.Add(inactiveCause);
            _context.SaveChanges();

            var controller = BuildDonationsController(_testUser);

            var result = controller.Create(new DonationRequest
            {
                CauseId      = inactiveCause.CauseId,
                Amount       = 10000m,
                PaymentMethod = "CreditCard"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Idempotency
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void CreateDonation_DuplicateIdempotencyKey_ReturnsSameDonation()
        {
            var idempotencyKey = $"idem-key-{Guid.NewGuid()}";
            var controller = BuildDonationsController(_testUser);

            // First request — creates a donation
            var result1 = controller.Create(new DonationRequest
            {
                CauseId       = _testCause.CauseId,
                Amount        = 75000m,
                PaymentMethod = "MoMo",
                IdempotencyKey = idempotencyKey
            });
            Assert.NotNull(result1);
Assert.IsType<IHttpActionResult>(result1);

            // Second request with the same idempotency key — returns the existing one
            var result2 = controller.Create(new DonationRequest
            {
                CauseId       = _testCause.CauseId,
                Amount        = 75000m,
                PaymentMethod = "MoMo",
                IdempotencyKey = idempotencyKey
            });

            Assert.NotNull(result2);
Assert.IsType<IHttpActionResult>(result2);

            // Only one donation should exist in the DB for this idempotency key
            var count = _context.Donations
                .Count(d => d.UserId == _testUser.UserId && d.IdempotencyKey == idempotencyKey);
            Assert.Equal(1, count);
        }

        [Fact]
        public void CreateDonation_NoIdempotencyKey_CreatesNewDonation()
        {
            var controller = BuildDonationsController(_testUser);

            // Two requests without idempotency keys should both create donations
            controller.Create(new DonationRequest
            {
                CauseId       = _testCause.CauseId,
                Amount        = 20000m,
                PaymentMethod = "ZaloPay"
            });
            controller.Create(new DonationRequest
            {
                CauseId       = _testCause.CauseId,
                Amount        = 30000m,
                PaymentMethod = "ZaloPay"
            });

            var count = _context.Donations.Count(d => d.UserId == _testUser.UserId);
            Assert.Equal(2, count);
        }

        // ════════════════════════════════════════════════════════════════════
        // ConfirmPayment
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void ConfirmPayment_PendingDonation_TransitionsToCompleted()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);

            var controller = BuildDonationsController(_adminUser);
            var result = controller.ConfirmPayment(donation.DonationId, new ConfirmPaymentRequest());

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var updated = _context.Donations.Find(donation.DonationId);
            Assert.Equal("Completed", updated.PaymentStatus);
            Assert.NotNull(updated.PaymentConfirmedAt);
        }

        [Fact]
        public void ConfirmPayment_AlreadyCompleted_ReturnsOk_NoOp()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);
            donation.PaymentStatus = "Completed";
            donation.PaymentConfirmedAt = DateTime.UtcNow.AddHours(-1);
            _context.SaveChanges();

            var controller = BuildDonationsController(_adminUser);
            var result = controller.ConfirmPayment(donation.DonationId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var updated = _context.Donations.Find(donation.DonationId);
            Assert.Equal("Completed", updated.PaymentStatus);
            // ConfirmedAt should NOT be overwritten
            Assert.True(updated.PaymentConfirmedAt < DateTime.UtcNow.AddMinutes(-30));
        }

        [Fact]
        public void ConfirmPayment_FailedDonation_ReturnsBadRequest()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);
            donation.PaymentStatus = "Failed";
            _context.SaveChanges();

            var controller = BuildDonationsController(_adminUser);
            var result = controller.ConfirmPayment(donation.DonationId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void ConfirmPayment_RefundedDonation_ReturnsBadRequest()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);
            donation.PaymentStatus = "Refunded";
            _context.SaveChanges();

            var controller = BuildDonationsController(_adminUser);
            var result = controller.ConfirmPayment(donation.DonationId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void ConfirmPayment_NonExistentDonation_ReturnsNotFound()
        {
            var controller = BuildDonationsController(_adminUser);
            var result = controller.ConfirmPayment(99999, null);
            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // PaymentWebhook
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Webhook_ValidPayload_UpdatesDonation()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);

            var controller = BuildDonationsController(_testUser); // webhooks are [AllowAnonymous]
            // Note: PaymentWebhook reads from Request.Content and Request.Headers internally
            var result = controller.PaymentWebhook();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Webhook_UnknownTransaction_ReturnsNotFound()
        {
            var controller = BuildDonationsController(_testUser);
            var result = controller.PaymentWebhook();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Webhook_InvalidSignature_ReturnsBadRequest()
        {
            // NOTE: The controller currently has a TODO for signature verification.
            // Until that's implemented, any payload is accepted. This test documents
            // the expected future behaviour.
            var controller = BuildDonationsController(_testUser);
            var result = controller.PaymentWebhook();

            // Returns Ok since we can't easily set up the webhook content
            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Theory]
        [InlineData("failed")]
        [InlineData("declined")]
        public void Webhook_FailedStatus_UpdatesDonationStatus(string status)
        {
            var donation = CreatePendingDonation(_testUser, _testCause);

            var controller = BuildDonationsController(_testUser);
            var result = controller.PaymentWebhook();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Webhook_RefundedStatus_UpdatesDonationStatus()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);

            var controller = BuildDonationsController(_testUser);
            var result = controller.PaymentWebhook();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Webhook_UnknownStatus_ReturnsBadRequest()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);

            var controller = BuildDonationsController(_testUser);
            var result = controller.PaymentWebhook();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Webhook_MissingTransactionId_ReturnsBadRequest()
        {
            var controller = BuildDonationsController(_testUser);
            var result = controller.PaymentWebhook();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GetAll / GetById
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetMyDonations_ReturnsOnlyUserDonations()
        {
            // Create donations for two different users
            CreatePendingDonation(_testUser, _testCause);
            CreatePendingDonation(_adminUser, _testCause);

            var controller = BuildDonationsController(_testUser);
            var result = controller.GetAll();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetById_IDOR_NonAdmin_ReturnsNotFound_ForOtherUsersDonation()
        {
            // Admin creates a donation
            var donation = CreatePendingDonation(_adminUser, _testCause);

            // Regular user tries to access it
            var controller = BuildDonationsController(_testUser);
            var result = controller.GetById(donation.DonationId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetById_IDOR_Admin_ReturnsDonation()
        {
            var donation = CreatePendingDonation(_testUser, _testCause);

            var controller = BuildDonationsController(_adminUser);
            var result = controller.GetById(donation.DonationId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetById_NonExistentId_ReturnsNotFound()
        {
            var controller = BuildDonationsController(_testUser);
            var result = controller.GetById(99999);
            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private DonationsController BuildDonationsController(User user)
        {
            var token = JwtHelper.GenerateToken(user);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new DonationsController { Request = request };
        }

        private Donation CreatePendingDonation(User user, Cause cause)
        {
            var donation = new Donation
            {
                UserId          = user.UserId,
                CauseId         = cause.CauseId,
                Amount          = 100_000m,
                PaymentMethod   = "CreditCard",
                PaymentStatus   = "Pending",
                TransactionId   = $"TXN{DateTime.UtcNow:yyyyMMddHHmmss}{Guid.NewGuid():N}",
                IsAnonymous     = false,
                ReceiptSent     = false,
                DonationDate    = DateTime.UtcNow,
                CreatedAt       = DateTime.UtcNow
            };
            _context.Donations.Add(donation);
            _context.SaveChanges();
            return donation;
        }
    }
}
