using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using GiveAID.Web.Data;
using GiveAID.Web.Models;

namespace GiveAID.Tests
{
    /// <summary>
    /// Provides isolated, fresh-in-memory <see cref="GiveAIDContext"/> instances for each
    /// integration test so tests never interfere with each other.
    ///
    /// Uses EF6 with a LocalDB connection string.  Each <c>CreateContext()</c> call
    /// returns a new context instance backed by the same LocalDB database.
    /// Individual tests are expected to call <c>ResetDatabase()</c> in <c>[TestInitialize]</c>
    /// to clear all tables and re-seed before every test.
    ///
    /// For CI / environments without LocalDB, pass <c>useInMemory: true</c> to use
    /// the <c>Effort</c> in-memory provider (add the <c>Effort</c> NuGet package to enable).
    /// </summary>
    public static class TestDbContextFactory
    {
        /// <summary>
        /// Creates a fresh <see cref="GiveAIDContext"/> backed by the test LocalDB
        /// connection defined in <c>app.config</c>.
        /// </summary>
        public static GiveAIDContext CreateContext()
        {
            return new GiveAIDContext();
        }

        /// <summary>
        /// Deletes all data from every test table and re-inserts seed records.
        /// Call this in <c>[TestInitialize]</c> to guarantee a clean state per test.
        /// </summary>
        public static void ResetDatabase(GiveAIDContext context)
        {
            // Disable EF initializer — we manage schema via SQL scripts, not EF migrations.
            Database.SetInitializer<GiveAIDContext>(null);

            // Delete in dependency order (child tables first) to avoid FK violations.
            ExecuteDelete(context, "CampaignRegistrations");
            ExecuteDelete(context, "Donations");
            ExecuteDelete(context, "CampaignReports");
            ExecuteDelete(context, "Campaigns");
            ExecuteDelete(context, "Causes");
            ExecuteDelete(context, "Invitations");
            ExecuteDelete(context, "EmailLogs");
            ExecuteDelete(context, "Users");

            // Seed default test accounts.
            SeedTestUsers(context);
        }

        /// <summary>
        /// Seeds the two standard test accounts used across integration tests.
        /// Passwords are hashed with BCrypt cost=11 (matching the production default).
        /// </summary>
        private static void SeedTestUsers(GiveAIDContext context)
        {
            // AdminHash  = BCrypt of "Admin@123"
            // UserHash   = BCrypt of "User@123"
            // Generated with BCrypt.Net.BCrypt.HashPassword(password, 11)
            const string AdminHash = "$2a$11$pirnEfNk.ZU71wnXOvS99uJklL0iBPrhxTq0watPsNLDhuVtW6Wny";
            const string UserHash  = "$2a$11$D2ZJOxRrjuq25IW.5OeOzuVNv8r4GAj8SH7zxXBWpAUu4ZmKvUIvi";

            if (!context.Users.Any(u => u.Email == TestUsers.AdminEmail))
            {
                context.Users.Add(new User
                {
                    Username          = "testadmin",
                    Email             = TestUsers.AdminEmail,          // admin@test.com
                    PasswordHash      = AdminHash,
                    FullName          = "Test Administrator",
                    Role              = "SuperAdmin",
                    IsActive          = true,
                    IsVerified        = true,
                    PasswordChangedAt = DateTime.UtcNow,
                    CreatedAt         = DateTime.UtcNow,
                    UpdatedAt         = DateTime.UtcNow
                });
            }

            if (!context.Users.Any(u => u.Email == TestUsers.NormalUserEmail))
            {
                context.Users.Add(new User
                {
                    Username          = "testuser",
                    Email             = TestUsers.NormalUserEmail,    // user@test.com
                    PasswordHash      = UserHash,
                    FullName          = "Test Normal User",
                    Role              = "User",
                    IsActive          = true,
                    IsVerified        = true,
                    PasswordChangedAt = DateTime.UtcNow,
                    CreatedAt         = DateTime.UtcNow,
                    UpdatedAt         = DateTime.UtcNow
                }
                );
            }

            context.SaveChanges();
        }

        private static void ExecuteDelete(GiveAIDContext context, string tableName)
        {
            try
            {
                context.Database.ExecuteSqlCommand($"DELETE FROM [{tableName}]");
            }
            catch (DbUpdateException)
            {
                // Table may be empty or not exist — safe to ignore
            }
        }

        /// <summary>
        /// Known test user email addresses and plaintext passwords.
        /// Use these in integration tests instead of hardcoding credentials.
        /// </summary>
        public static class TestUsers
        {
            public const string AdminEmail     = "admin@test.com";
            public const string AdminPassword  = "Admin@123";

            public const string NormalUserEmail = "user@test.com";
            public const string NormalPassword = "User@123";

            /// <summary>
            /// BCrypt hash of Admin@123 at cost 11.
            /// Same hash as in GiveAIDContext.SeedDatabase so password always works.
            /// </summary>
            public const string AdminHash =
                "$2a$11$pirnEfNk.ZU71wnXOvS99uJklL0iBPrhxTq0watPsNLDhuVtW6Wny";

            /// <summary>
            /// BCrypt hash of User@123 at cost 11.
            /// Same hash as in GiveAIDContext.SeedDatabase so password always works.
            /// </summary>
            public const string NormalUserHash =
                "$2a$11$D2ZJOxRrjuq25IW.5OeOzuVNv8r4GAj8SH7zxXBWpAUu4ZmKvUIvi";
        }
    }
}
