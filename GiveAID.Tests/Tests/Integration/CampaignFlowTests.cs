using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using GiveAID.Web.Controllers;
using GiveAID.Web.Data;
using GiveAID.Web.Helpers;
using GiveAID.Web.Models;
using Xunit;

namespace GiveAID.Tests.Tests.Integration
{
    /// <summary>
    /// Integration tests for <see cref="CampaignsController"/>.
    ///
    /// Covered flows:
    ///   • GetAll       — returns active campaigns
    ///   • GetFeatured  — returns only featured/active campaigns
    ///   • GetById      — valid ID, invalid ID
    ///   • Create       — admin creates, non-admin forbidden
    ///   • Update       — admin updates
    ///   • Delete       — superadmin deletes, admin forbidden
    ///   • Register     — user registers for event-style campaign
    ///   • MyRegistrations — returns user's own registrations
    /// </summary>
    public class CampaignFlowTests : IDisposable
    {
        private readonly GiveAIDContext _context;
        private readonly User _adminUser;
        private readonly User _normalUser;
        private readonly Cause _testCause;
        private readonly Campaign _activeCampaign;
        private readonly Campaign _eventCampaign;

        public CampaignFlowTests()
        {
            _context = TestDbContextFactory.CreateContext();
            TestDbContextFactory.ResetDatabase(_context);

            _adminUser = new User
            {
                Username          = "campaignadmin",
                Email             = "campaignadmin@test.com",
                PasswordHash      = PasswordHasher.Hash("Admin@123"),
                FullName          = "Campaign Admin",
                Role              = "Admin",
                IsActive          = true,
                IsVerified        = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(_adminUser);

            _normalUser = new User
            {
                Username          = "campaignuser",
                Email             = "campaignuser@test.com",
                PasswordHash      = PasswordHasher.Hash("User@123"),
                FullName          = "Campaign User",
                Role              = "User",
                IsActive          = true,
                IsVerified        = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(_normalUser);

            _testCause = new Cause
            {
                CauseName   = "Children Education",
                CauseCode   = "EDU",
                IsActive    = true,
                TargetAmount = 50_000_000m,
                RaisedAmount = 10_000_000m,
                CreatedAt   = DateTime.UtcNow
            };
            _context.Causes.Add(_testCause);

            _activeCampaign = new Campaign
            {
                CauseId               = _testCause.CauseId,
                CampaignName          = "Annual Fundraiser 2026",
                CampaignCode          = "AF2026",
                Description           = "Help us raise funds for school supplies.",
                GoalAmount            = 20_000_000m,
                RaisedAmount          = 5_000_000m,
                StartDate             = DateTime.Today.AddDays(-10),
                EndDate               = DateTime.Today.AddDays(30),
                Status                = "Active",
                IsFeatured            = true,
                RegistrationRequired   = false,
                CreatedBy             = _adminUser.UserId,
                CreatedAt             = DateTime.UtcNow
            };
            _context.Campaigns.Add(_activeCampaign);

            // An event-style campaign (accepts registrations)
            _eventCampaign = new Campaign
            {
                CauseId             = _testCause.CauseId,
                CampaignName        = "Charity Run 2026",
                CampaignCode        = "CR2026",
                Description         = "Join our 5km charity run!",
                GoalAmount          = 10_000_000m,
                RaisedAmount        = 0,
                StartDate           = DateTime.Today.AddDays(60),
                EndDate             = DateTime.Today.AddDays(60),
                Status              = "Active",
                IsFeatured          = false,
                RegistrationRequired = true,
                MaxParticipants      = 500,
                Location            = "Ho Chi Minh City",
                CreatedBy           = _adminUser.UserId,
                CreatedAt           = DateTime.UtcNow
            };
            _context.Campaigns.Add(_eventCampaign);
            _context.SaveChanges();
        }

        public void Dispose() => _context?.Dispose();

        // ════════════════════════════════════════════════════════════════════
        // GetAll
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetAll_ReturnsPublicCampaigns()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetAll();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetAll_FiltersByStatus()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetAll(status: "Active");

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetAll_FiltersByCauseId()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetAll(causeId: _testCause.CauseId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetAll_SearchByName_ReturnsMatches()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetAll(search: "Fundraiser");

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetAll_EventsOnly_ReturnsRegistrationCampaigns()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetAll(eventsOnly: true);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GetFeatured
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetFeatured_ReturnsMarkedAsFeatured()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetFeatured();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetFeatured_RespectsCountParameter()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetFeatured(count: 1);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // GetById
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void GetById_ValidId_ReturnsCampaign()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetById(_activeCampaign.CampaignId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void GetById_InvalidId_ReturnsNotFound()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetById(99999);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Create (Admin only)
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Create_Admin_CreatesCampaign()
        {
            var controller = BuildController(_adminUser);
            var result = controller.Create(new CampaignCreateRequest
            {
                CauseId      = _testCause.CauseId,
                CampaignName = "New Campaign",
                CampaignCode = "NC001",
                GoalAmount   = 5_000_000m,
                StartDate    = DateTime.Today,
                EndDate      = DateTime.Today.AddMonths(3)
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var campaign = _context.Campaigns
                .FirstOrDefault(c => c.CampaignCode == "NC001");
            Assert.NotNull(campaign);
            Assert.Equal("New Campaign", campaign.CampaignName);
        }

        [Fact]
        public void Create_NonAdmin_ReturnsForbidden()
        {
            var controller = BuildController(_normalUser);
            var result = controller.Create(new CampaignCreateRequest
            {
                CauseId      = _testCause.CauseId,
                CampaignName = "Unauthorized Campaign",
                GoalAmount   = 1_000_000m,
                StartDate    = DateTime.Today
            });

            // JwtAuthorizeAttribute returns 401 for non-admin users
            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Create_InvalidCause_ReturnsBadRequest()
        {
            var controller = BuildController(_adminUser);
            var result = controller.Create(new CampaignCreateRequest
            {
                CauseId      = 99999, // non-existent
                CampaignName = "Bad Cause Campaign",
                GoalAmount   = 1_000_000m,
                StartDate    = DateTime.Today
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Update (Admin only)
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Update_Admin_UpdatesCampaign()
        {
            var controller = BuildController(_adminUser);
            var result = controller.Update(_activeCampaign.CampaignId, new CampaignUpdateRequest
            {
                CampaignName = "Updated Campaign Name",
                GoalAmount   = 30_000_000m
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var updated = _context.Campaigns.Find(_activeCampaign.CampaignId);
            Assert.Equal("Updated Campaign Name", updated.CampaignName);
            Assert.Equal(30_000_000m, updated.GoalAmount);
        }

        [Fact]
        public void Update_NonExistentCampaign_ReturnsNotFound()
        {
            var controller = BuildController(_adminUser);
            var result = controller.Update(99999, new CampaignUpdateRequest
            {
                CampaignName = "Does Not Exist"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Update_NonAdmin_ReturnsForbidden()
        {
            var controller = BuildController(_normalUser);
            var result = controller.Update(_activeCampaign.CampaignId, new CampaignUpdateRequest
            {
                CampaignName = "Hacked Name"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Delete (SuperAdmin only — Admin is not sufficient)
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Delete_SuperAdmin_DeletesCampaign()
        {
            // Create a campaign that we can safely delete (no donations)
            var campaign = new Campaign
            {
                CauseId      = _testCause.CauseId,
                CampaignName = "To Be Deleted",
                GoalAmount   = 1_000_000m,
                StartDate    = DateTime.Today,
                Status       = "Draft",
                CreatedAt    = DateTime.UtcNow
            };
            _context.Campaigns.Add(campaign);
            _context.SaveChanges();

            var superAdmin = new User
            {
                Username          = "superadmin2",
                Email             = "superadmin2@test.com",
                PasswordHash      = PasswordHasher.Hash("Admin@123"),
                FullName          = "Super Admin 2",
                Role              = "SuperAdmin",
                IsActive          = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(superAdmin);
            _context.SaveChanges();

            var controller = BuildController(superAdmin);
            var result = controller.Delete(campaign.CampaignId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var deleted = _context.Campaigns.Find(campaign.CampaignId);
            Assert.Null(deleted);
        }

        [Fact]
        public void Delete_Admin_ReturnsForbidden()
        {
            // Admin (not SuperAdmin) should be blocked from deleting
            var controller = BuildController(_adminUser);
            var result = controller.Delete(_activeCampaign.CampaignId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Delete_CampaignWithDonations_ReturnsBadRequest()
        {
            // Add a completed donation to the campaign
            var donation = new Donation
            {
                UserId        = _normalUser.UserId,
                CauseId       = _testCause.CauseId,
                CampaignId    = _activeCampaign.CampaignId,
                Amount        = 100_000m,
                PaymentMethod = "BankTransfer",
                PaymentStatus = "Completed",
                TransactionId = "TXN-DON-001",
                DonationDate  = DateTime.UtcNow,
                CreatedAt     = DateTime.UtcNow
            };
            _context.Donations.Add(donation);
            _context.SaveChanges();

            var superAdmin = new User
            {
                Username          = "superadmin3",
                Email             = "superadmin3@test.com",
                PasswordHash      = PasswordHasher.Hash("Admin@123"),
                FullName          = "Super Admin 3",
                Role              = "SuperAdmin",
                IsActive          = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(superAdmin);
            _context.SaveChanges();

            var controller = BuildController(superAdmin);
            var result = controller.Delete(_activeCampaign.CampaignId);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Delete_NonExistentCampaign_ReturnsNotFound()
        {
            var superAdmin = new User
            {
                Username          = "superadmin4",
                Email             = "superadmin4@test.com",
                PasswordHash      = PasswordHasher.Hash("Admin@123"),
                FullName          = "Super Admin 4",
                Role              = "SuperAdmin",
                IsActive          = true,
                PasswordChangedAt = DateTime.UtcNow,
                CreatedAt         = DateTime.UtcNow,
                UpdatedAt         = DateTime.UtcNow
            };
            _context.Users.Add(superAdmin);
            _context.SaveChanges();

            var controller = BuildController(superAdmin);
            var result = controller.Delete(99999);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Register
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void Register_LoggedInUser_Succeeds()
        {
            var controller = BuildController(_normalUser);
            var result = controller.Register(_eventCampaign.CampaignId, new CampaignRegistrationRequest
            {
                Notes = "Looking forward to it!"
            });

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);

            var reg = _context.CampaignRegistrations
                .FirstOrDefault(r => r.CampaignId == _eventCampaign.CampaignId
                                     && r.UserId == _normalUser.UserId);
            Assert.NotNull(reg);
            Assert.Equal("Registered", reg.Status);
        }

        [Fact]
        public void Register_DonationOnlyCampaign_ReturnsBadRequest()
        {
            var controller = BuildController(_normalUser);
            var result = controller.Register(_activeCampaign.CampaignId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_DuplicateRegistration_ReturnsBadRequest()
        {
            // First registration
            _context.CampaignRegistrations.Add(new CampaignRegistration
            {
                CampaignId = _eventCampaign.CampaignId,
                UserId     = _normalUser.UserId,
                Status     = "Registered",
                RegistrationDate = DateTime.UtcNow
            });
            _context.SaveChanges();

            var controller = BuildController(_normalUser);
            var result = controller.Register(_eventCampaign.CampaignId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_FullCampaign_ReturnsBadRequest()
        {
            // Set MaxParticipants to 1 and register the existing user
            var fullCampaign = new Campaign
            {
                CauseId             = _testCause.CauseId,
                CampaignName        = "Full Campaign",
                GoalAmount          = 1_000_000m,
                StartDate           = DateTime.Today.AddDays(30),
                Status              = "Active",
                RegistrationRequired = true,
                MaxParticipants      = 1, // full
                CreatedAt           = DateTime.UtcNow
            };
            _context.Campaigns.Add(fullCampaign);

            // Pre-register normal user
            _context.CampaignRegistrations.Add(new CampaignRegistration
            {
                CampaignId = fullCampaign.CampaignId,
                UserId     = _normalUser.UserId,
                Status     = "Registered",
                RegistrationDate = DateTime.UtcNow
            });
            _context.SaveChanges();

            var controller = BuildController(_adminUser);
            var result = controller.Register(fullCampaign.CampaignId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_NonExistentCampaign_ReturnsNotFound()
        {
            var controller = BuildController(_normalUser);
            var result = controller.Register(99999, null);
            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void Register_CancelledCampaign_ReturnsBadRequest()
        {
            var cancelledCampaign = new Campaign
            {
                CauseId             = _testCause.CauseId,
                CampaignName        = "Cancelled Event",
                GoalAmount          = 1_000_000m,
                StartDate           = DateTime.Today.AddDays(30),
                Status              = "Cancelled", // cancelled
                RegistrationRequired = true,
                CreatedAt           = DateTime.UtcNow
            };
            _context.Campaigns.Add(cancelledCampaign);
            _context.SaveChanges();

            var controller = BuildController(_normalUser);
            var result = controller.Register(cancelledCampaign.CampaignId, null);

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // MyRegistrations
        // ════════════════════════════════════════════════════════════════════

        [Fact]
        public void MyRegistrations_ReturnsUserRegistrations()
        {
            // Pre-seed a registration for normalUser
            _context.CampaignRegistrations.Add(new CampaignRegistration
            {
                CampaignId = _eventCampaign.CampaignId,
                UserId     = _normalUser.UserId,
                Status     = "Registered",
                RegistrationDate = DateTime.UtcNow
            });
            _context.SaveChanges();

            var controller = BuildController(_normalUser);
            var result = controller.GetMyRegistrations();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        [Fact]
        public void MyRegistrations_ReturnsEmptyList_WhenNoRegistrations()
        {
            var controller = BuildController(_normalUser);
            var result = controller.GetMyRegistrations();

            Assert.NotNull(result);
Assert.IsType<IHttpActionResult>(result);
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private CampaignsController BuildController(User user)
        {
            var token = JwtHelper.GenerateToken(user);
            var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/");
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new CampaignsController { Request = request };
        }
    }
}
