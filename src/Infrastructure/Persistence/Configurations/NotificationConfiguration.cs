using GiveAID.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(x => x.NotificationId);

        builder.Property(x => x.NotificationId)
            .HasColumnName("notification_id");

        builder.Property(x => x.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(x => x.Type)
            .HasColumnName("type")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Title)
            .HasColumnName("title")
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Message)
            .HasColumnName("message")
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(x => x.RelatedEntityType)
            .HasColumnName("related_entity_type")
            .HasMaxLength(50);

        builder.Property(x => x.RelatedEntityId)
            .HasColumnName("related_entity_id");

        builder.Property(x => x.IsRead)
            .HasColumnName("is_read")
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(x => x.ReadAt)
            .HasColumnName("read_at");

        builder.Property(x => x.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(x => x.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Property(x => x.IsDeleted)
            .HasColumnName("is_deleted");

        builder.Property(x => x.DeletedAt)
            .HasColumnName("deleted_at");

        // Soft delete filter
        builder.HasQueryFilter(x => !x.IsDeleted);

        // Navigation
        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for querying user notifications
        builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
    }
}
