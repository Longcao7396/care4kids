using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.ToTable("achievements");

        builder.HasKey(a => a.AchievementId);

        builder.Property(a => a.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(a => a.Category)
            .HasMaxLength(100);

        builder.Property(a => a.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.MetricValue)
            .HasPrecision(18, 2);

        builder.Property(a => a.MetricLabel)
            .HasMaxLength(100);

        builder.Property(a => a.MetricSuffix)
            .HasMaxLength(20);

        builder.Property(a => a.ImageUrl)
            .HasMaxLength(500);

        builder.Property(a => a.Icon)
            .HasMaxLength(50);

        builder.Property(a => a.AwardBy)
            .HasMaxLength(150);

        builder.Property(a => a.Location)
            .HasMaxLength(200);

        builder.HasOne(a => a.CreatedByUser)
            .WithMany(u => u.CreatedAchievements)
            .HasForeignKey(a => a.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(a => a.IsFeatured);
        builder.HasIndex(a => a.DisplayOrder);
    }
}
