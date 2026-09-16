using System;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text.RegularExpressions;
using GiveAID.Web.Models;

namespace GiveAID.Web.Data
{
    public class GiveAIDContext : DbContext
    {
        public GiveAIDContext() : base("name=GiveAIDContext")
        {
            // The SQL script is the canonical schema; do not let EF create a divergent database.
            Database.SetInitializer<GiveAIDContext>(null);
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        public DbSet<Cause> Causes { get; set; }
        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<CampaignReport> CampaignReports { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<CampaignRegistration> CampaignRegistrations { get; set; }
        // Legacy entities — kept for backward compatibility (Gallery FK references
        // Programmes.programme_id; data is read-only through ProgrammesController).
        public DbSet<Programme> Programmes { get; set; }
        public DbSet<ProgrammePhoto> ProgrammePhotos { get; set; }
        public DbSet<ProgrammeRegistration> ProgrammeRegistrations { get; set; }
        public DbSet<Conversation> Conversations { get; set; }
        public DbSet<ConversationMessage> ConversationMessages { get; set; }
        public DbSet<CmsPage> CmsPages { get; set; }
        public DbSet<Career> Careers { get; set; }
        public DbSet<CareerApplication> CareerApplications { get; set; }
        public DbSet<Gallery> Gallery { get; set; }
        public DbSet<ContactMessage> ContactMessages { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Achievement> Achievements { get; set; }
        public DbSet<Faq> Faqs { get; set; }
        public DbSet<Invitation> Invitations { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // The canonical SQL schema uses snake_case columns while C# uses PascalCase.
            modelBuilder.Conventions.Add(new SnakeCaseColumnNameConvention());

            base.OnModelCreating(modelBuilder);

            // User configurations
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Donation configurations
            modelBuilder.Entity<Donation>()
                .Property(d => d.Amount)
                .HasPrecision(18, 2);

            // Idempotency: (UserId, IdempotencyKey) is unique when IdempotencyKey
            // is non-null. EF6 does not support filtered indexes, so we use a
            // composite index and rely on application logic to set IdempotencyKey
            // only when the client provides one. Multiple null keys per user are
            // still allowed.
            modelBuilder.Entity<Donation>()
                .HasIndex(d => new { d.UserId, d.IdempotencyKey })
                .IsUnique();

            // Programme registration: prevent double-booking at the DB level.
            // (ProgrammeId, UserId) composite unique index lets two concurrent
            // Register requests race and one fails cleanly with DbUpdateException,
            // which the controller converts to a 400 instead of an unsafe
            // Serializable transaction.
            modelBuilder.Entity<ProgrammeRegistration>()
                .HasIndex(r => new { r.ProgrammeId, r.UserId })
                .IsUnique();

            // Same for CampaignRegistrations: (CampaignId, UserId) unique.
            modelBuilder.Entity<CampaignRegistration>()
                .HasIndex(r => new { r.CampaignId, r.UserId })
                .IsUnique();

            // Cause configurations
            modelBuilder.Entity<Cause>()
                .Property(c => c.TargetAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Cause>()
                .Property(c => c.RaisedAmount)
                .HasPrecision(18, 2);

            // Campaign configurations
            modelBuilder.Entity<Campaign>()
                .Property(c => c.GoalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Campaign>()
                .Property(c => c.RaisedAmount)
                .HasPrecision(18, 2);

            // Programme fields merged into Campaign
            modelBuilder.Entity<Campaign>()
                .Property(c => c.ExpectedBudget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Campaign>()
                .Property(c => c.ActualBudget)
                .HasPrecision(18, 2);

            // CampaignRegistration configurations
            modelBuilder.Entity<CampaignRegistration>()
                .HasIndex(r => new { r.CampaignId, r.UserId })
                .IsUnique();

            // Campaign Report configurations
            modelBuilder.Entity<CampaignReport>()
                .Property(r => r.TotalReceived)
                .HasPrecision(18, 2);

            modelBuilder.Entity<CampaignReport>()
                .Property(r => r.TotalSpent)
                .HasPrecision(18, 2);

            // Programme configurations
            modelBuilder.Entity<Programme>()
                .Property(p => p.ExpectedBudget)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Programme>()
                .Property(p => p.ActualBudget)
                .HasPrecision(18, 2);

            // Achievement configurations
            modelBuilder.Entity<Achievement>()
                .Property(a => a.MetricValue)
                .HasPrecision(18, 2);
        }

        private sealed class SnakeCaseColumnNameConvention : IStoreModelConvention<EdmProperty>
        {
            public void Apply(EdmProperty item, DbModel model)
            {
                item.Name = ToSnakeCase(item.Name);
            }
        }

        private static string ToSnakeCase(string name)
        {
            return Regex.Replace(name, "([a-z0-9])([A-Z])", "$1_$2").ToLowerInvariant();
        }

        public static void SeedDatabase(GiveAIDContext context)
        {
            // Causes, campaigns and donations are seeded exclusively by the
            // *.sql scripts in the project root (Campaigns_DataSeed.sql,
            // NGO_Database_Causes_Restructure_Migration.sql, etc.). Do NOT
            // add data seeding here — it would diverge from the canonical schema.

            // BCrypt hashes for the two default accounts (cost = 11).
            // Generated once with BCrypt.Net.BCrypt.HashPassword(plain, 11) and
            // stored here as literals so SeedDatabase never needs to call the
            // expensive BCrypt algorithm at startup.
            // Verified against "Admin@123" / "User@123" on 2026-09-16.
            const string AdminHash = "$2a$11$pirnEfNk.ZU71wnXOvS99uJklL0iBPrhxTq0watPsNLDhuVtW6Wny"; // Admin@123
            const string UserHash  = "$2a$11$D2ZJOxRrjuq25IW.5OeOzuVNv8r4GAj8SH7zxXBWpAUu4ZmKvUIvi"; // User@123

            // Admin user — idempotent upsert so default password always works
            // even after schema resets.
            var admin = context.Users.FirstOrDefault(u => u.Email == "admin@give-aid.org");
            if (admin == null)
            {
                context.Users.Add(new User
                {
                    Username = "admin",
                    Email = "admin@give-aid.org",
                    PasswordHash = AdminHash,
                    FullName = "System Administrator",
                    Role = "SuperAdmin",
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                // Keep the demo credentials working across DB rebuilds.
                admin.PasswordHash = AdminHash;
                admin.IsActive = true;
                admin.IsVerified = true;
            }
            context.SaveChanges();

            // Demo user — same idempotent upsert pattern.
            var demoUser = context.Users.FirstOrDefault(u => u.Email == "user@example.com");
            if (demoUser == null)
            {
                context.Users.Add(new User
                {
                    Username = "demouser",
                    Email = "user@example.com",
                    PasswordHash = UserHash,
                    FullName = "Demo User",
                    Role = "User",
                    IsActive = true,
                    IsVerified = true,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                demoUser.PasswordHash = UserHash;
                demoUser.IsActive = true;
                demoUser.IsVerified = true;
            }
            context.SaveChanges();
        }
    }
}
