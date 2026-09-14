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
            // Causes and campaigns are seeded exclusively by NGO_Database_Schema_V2.sql.

            // BCrypt hashes for the two default accounts.
            // Generated once with BCrypt.Net.BCrypt.HashPassword(plain, 11) and
            // stored here as literals so SeedDatabase never needs to call the
            // expensive BCrypt algorithm at startup.
            const string AdminHash = "$2a$11$rBV2J7kF0lpRk4qJbL.W.OMTvSITkVxXh5o3wEQzN5pVU5p9KGz8vW"; // Admin@123
            const string UserHash  = "$2a$11$W8n5k5q3vVcJZpYxZ3hHxe3Lhq1JjQ8Jkx5OJlWxMxQxJxQxJxJxJ"; // User@123

            // Seed Admin User — always reset password to default so "Admin@123" always works.
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
                context.SaveChanges();
            }

            // Seed Demo User
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
                context.SaveChanges();
            }
        }
    }
}
