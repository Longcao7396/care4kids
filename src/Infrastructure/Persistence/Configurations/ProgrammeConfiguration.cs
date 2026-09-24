using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class ProgrammeConfiguration : IEntityTypeConfiguration<Programme>
{
    public void Configure(EntityTypeBuilder<Programme> builder)
    {
        builder.ToTable("programmes");

        builder.HasKey(p => p.ProgrammeId);

        builder.Property(p => p.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.ProgrammeType)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.ImageUrl)
            .HasMaxLength(255);

        builder.Property(p => p.Location)
            .HasMaxLength(255);

        builder.Property(p => p.ExpectedBudget)
            .HasPrecision(18, 2);

        builder.Property(p => p.ActualBudget)
            .HasPrecision(18, 2);

        builder.Property(p => p.Status)
            .HasMaxLength(20)
            .IsRequired();

        builder.HasOne(p => p.Organization)
            .WithMany(o => o.Programmes)
            .HasForeignKey(p => p.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(p => p.Status);
        builder.HasIndex(p => p.IsFeatured);
    }
}
