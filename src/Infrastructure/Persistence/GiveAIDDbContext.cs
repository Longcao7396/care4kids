using Microsoft.EntityFrameworkCore;
using GiveAID.Application.Common.Interfaces;
using GiveAID.Application.Services;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence;

public class GiveAIDDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService? _currentUserService;

    public GiveAIDDbContext(
        DbContextOptions<GiveAIDDbContext> options,
        ICurrentUserService? currentUserService = null) : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Cause> Causes => Set<Cause>();
    public DbSet<Campaign> Campaigns => Set<Campaign>();
    public DbSet<CampaignRegistration> CampaignRegistrations => Set<CampaignRegistration>();
    public DbSet<CampaignReport> CampaignReports => Set<CampaignReport>();
    public DbSet<Donation> Donations => Set<Donation>();
    public DbSet<Programme> Programmes => Set<Programme>();
    public DbSet<ProgrammeRegistration> ProgrammeRegistrations => Set<ProgrammeRegistration>();
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
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply snake_case naming convention to match the canonical SQL schema
        SnakeCaseNamingConvention.ApplyToModel(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GiveAIDDbContext).Assembly);

        // Global query filter for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = System.Linq.Expressions.Expression.Parameter(entityType.ClrType, "e");
                var property = System.Linq.Expressions.Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var falseConstant = System.Linq.Expressions.Expression.Constant(false);
                var filter = System.Linq.Expressions.Expression.Equal(property, falseConstant);
                var lambda = System.Linq.Expressions.Expression.Lambda(filter, parameter);
                entityType.SetQueryFilter(lambda);
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var currentUserId = _currentUserService?.GetUserId();

        // Capture audit entries before saving
        var auditEntries = new List<AuditLog>();

        // Auto-set CreatedAt, UpdatedAt, CreatedBy, UpdatedBy and capture audit logs
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted);

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = utcNow;
                    if (currentUserId != null)
                    {
                        baseEntity.CreatedBy = currentUserId;
                    }

                    // Audit log for Create
                    auditEntries.Add(CreateAuditLog(entry, "Create", currentUserId, utcNow));
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = utcNow;
                    if (currentUserId != null)
                    {
                        baseEntity.UpdatedBy = currentUserId;
                    }

                    // Audit log for Update
                    auditEntries.Add(CreateAuditLog(entry, "Update", currentUserId, utcNow));
                }
                else if (entry.State == EntityState.Deleted)
                {
                    // Intercept hard delete and convert to soft delete
                    entry.State = EntityState.Modified;
                    baseEntity.IsDeleted = true;
                    baseEntity.DeletedAt = utcNow;
                    baseEntity.UpdatedAt = utcNow;
                    if (currentUserId != null)
                    {
                        baseEntity.UpdatedBy = currentUserId;
                    }

                    // Audit log for Delete
                    auditEntries.Add(CreateAuditLog(entry, "Delete", currentUserId, utcNow));
                }
            }
        }

        // Add audit logs to context before saving
        if (auditEntries.Any())
        {
            AuditLogs.AddRange(auditEntries);
        }

        return base.SaveChangesAsync(cancellationToken);
    }

    private AuditLog CreateAuditLog(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry, string action, string? userId, DateTime timestamp)
    {
        var entityType = entry.Entity.GetType().Name;
        var entityId = GetPrimaryKeyValue(entry);

        var oldValues = action == "Create" ? null : SerializeEntity(entry.OriginalValues);
        var newValues = action == "Delete" ? null : SerializeEntity(entry.CurrentValues);

        return new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityType = entityType,
            EntityId = entityId,
            OldValues = oldValues,
            NewValues = newValues,
            Timestamp = timestamp,
            IpAddress = null, // Will be set by middleware if needed
            UserAgent = null  // Will be set by middleware if needed
        };
    }

    private string? GetPrimaryKeyValue(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
    {
        var keyName = entry.Metadata.FindPrimaryKey()?.Properties
            .Select(x => x.Name).FirstOrDefault();

        if (keyName == null) return null;

        var keyValue = entry.Property(keyName).CurrentValue;
        return keyValue?.ToString();
    }

    private string? SerializeEntity(Microsoft.EntityFrameworkCore.ChangeTracking.PropertyValues values)
    {
        try
        {
            var dict = new Dictionary<string, object?>();
            foreach (var property in values.Properties)
            {
                // Skip audit fields and navigation properties
                if (property.Name is "CreatedAt" or "UpdatedAt" or "CreatedBy" or "UpdatedBy" or "IsDeleted" or "DeletedAt")
                    continue;

                dict[property.Name] = values[property];
            }
            return System.Text.Json.JsonSerializer.Serialize(dict);
        }
        catch
        {
            return null;
        }
    }
}
