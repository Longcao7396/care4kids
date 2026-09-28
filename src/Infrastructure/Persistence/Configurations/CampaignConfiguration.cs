using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CampaignConfiguration : IEntityTypeConfiguration<Campaign>
{
    public void Configure(EntityTypeBuilder<Campaign> builder)
    {
        builder.ToTable("campaigns");

        builder.HasKey(c => c.CampaignId);

        builder.Property(c => c.CampaignName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.CampaignCode)
            .HasMaxLength(50);

        builder.Property(c => c.ProgrammeType)
            .HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.GoalAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.RaisedAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.ExpectedBudget)
            .HasPrecision(18, 2);

        builder.Property(c => c.ActualBudget)
            .HasPrecision(18, 2);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(500);

        builder.Property(c => c.Location)
            .HasMaxLength(200);

        builder.Property(c => c.Status)
            .HasMaxLength(20)
            .IsRequired();

        // Relationships
        builder.HasOne(c => c.Cause)
            .WithMany(ca => ca.Campaigns)
            .HasForeignKey(c => c.CauseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Organization)
            .WithMany(o => o.Campaigns)
            .HasForeignKey(c => c.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.CauseId);
        builder.HasIndex(c => c.Status);
        builder.HasIndex(c => c.IsFeatured);

        // Soft delete: hide deleted campaigns from normal queries.
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
