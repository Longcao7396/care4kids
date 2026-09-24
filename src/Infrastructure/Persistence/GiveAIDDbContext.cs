using Microsoft.EntityFrameworkCore;
using GiveAID.Application.Common.Interfaces;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence;

public class GiveAIDDbContext : DbContext, IApplicationDbContext
{
    public GiveAIDDbContext(DbContextOptions<GiveAIDDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Cause> Causes => Set<Cause>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignRegistration> CampaignRegistrations => Set<CampaignRegistration>();
    public DbSet<CampaignReport> CampaignReports => Set<CampaignReport>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Programme> Programmes => Set<Programme>();
    public DbSet<ProgrammeRegistration> ProgrammeRegistrations => Set<ProgrammeRegistration>();
    public DbSet<ProgrammePhoto> ProgrammePhotos => Set<ProgrammePhoto>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<ConversationMessage> ConversationMessages => Set<ConversationMessage>();
    public DbSet<CmsPage> CmsPages => Set<CmsPage>();
    public DbSet<Career> Careers => Set<Career>();
    public DbSet<CareerApplication> CareerApplications => Set<CareerApplication>();
    public DbSet<Gallery> Gallery => Set<Gallery>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();
    public DbSet<Achievement> Achievements => Set<Achievement>();
    public DbSet<Faq> Faqs => Set<Faq>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<WebhookLog> WebhookLogs => Set<WebhookLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply snake_case naming convention to match the canonical SQL schema
        SnakeCaseNamingConvention.ApplyToModel(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiveAIDDbContext).Assembly);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-set CreatedAt and UpdatedAt for entities
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                }
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
