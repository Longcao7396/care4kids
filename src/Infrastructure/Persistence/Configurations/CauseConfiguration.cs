using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CauseConfiguration : IEntityTypeConfiguration<Cause>
{
    public void Configure(EntityTypeBuilder<Cause> builder)
    {
        builder.ToTable("causes");

        builder.HasKey(c => c.CauseId);

        builder.Property(c => c.CauseCode)
            .HasMaxLength(20);

        builder.Property(c => c.CauseName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(500);

        builder.Property(c => c.ImageUrl)
            .HasMaxLength(255);

        builder.Property(c => c.Icon)
            .HasMaxLength(50);

        builder.Property(c => c.TargetAmount)
            .HasPrecision(18, 2);

        builder.Property(c => c.RaisedAmount)
            .HasPrecision(18, 2);

        // Self-referencing relationship for parent/child causes
        builder.HasOne(c => c.ParentCause)
            .WithMany(c => c.SubCauses)
            .HasForeignKey(c => c.ParentCauseId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => c.CauseCode);
        builder.HasIndex(c => c.DisplayOrder);
    }
}
