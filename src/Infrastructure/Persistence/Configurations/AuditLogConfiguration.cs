using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("audit_logs");

        builder.HasKey(a => a.AuditLogId);

        builder.Property(a => a.AuditLogId)
            .HasColumnName("audit_log_id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .HasMaxLength(450);

        builder.Property(a => a.Action)
            .IsRequired()
            .HasColumnName("action")
            .HasMaxLength(50);

        builder.Property(a => a.EntityType)
            .HasColumnName("entity_type")
            .HasMaxLength(100);

        builder.Property(a => a.EntityId)
            .HasColumnName("entity_id")
            .HasMaxLength(450);

        builder.Property(a => a.OldValues)
            .HasColumnName("old_values")
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.NewValues)
            .HasColumnName("new_values")
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.Timestamp)
            .IsRequired()
            .HasColumnName("timestamp")
            .HasColumnType("datetime2");

        builder.Property(a => a.IpAddress)
            .HasColumnName("ip_address")
            .HasMaxLength(45);

        builder.Property(a => a.UserAgent)
            .HasColumnName("user_agent")
            .HasMaxLength(500);

        // Index for common queries
        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("idx_audit_logs_user_id");

        builder.HasIndex(a => a.Timestamp)
            .HasDatabaseName("idx_audit_logs_timestamp");

        builder.HasIndex(a => new { a.EntityType, a.EntityId })
            .HasDatabaseName("idx_audit_logs_entity");
    }
}
