using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class GalleryConfiguration : IEntityTypeConfiguration<Gallery>
{
    public void Configure(EntityTypeBuilder<Gallery> builder)
    {
        builder.ToTable("gallery");

        builder.HasKey(g => g.GalleryId);

        builder.Property(g => g.Title)
            .HasMaxLength(200);

        builder.Property(g => g.PhotoUrl)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(g => g.ThumbnailUrl)
            .HasMaxLength(255);

        builder.Property(g => g.Category)
            .HasMaxLength(50);

        builder.Property(g => g.Tags)
            .HasMaxLength(255);

        builder.HasOne(g => g.Programme)
            .WithMany(p => p.GalleryItems)
            .HasForeignKey(g => g.ProgrammeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(g => g.Organization)
            .WithMany(o => o.GalleryItems)
            .HasForeignKey(g => g.OrganizationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(g => g.Category);
        builder.HasIndex(g => g.IsFeatured);
    }
}
