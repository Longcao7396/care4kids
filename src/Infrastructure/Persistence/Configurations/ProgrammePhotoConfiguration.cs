using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class ProgrammePhotoConfiguration : IEntityTypeConfiguration<ProgrammePhoto>
{
    public void Configure(EntityTypeBuilder<ProgrammePhoto> builder)
    {
        builder.ToTable("programme_photos");

        builder.HasKey(p => p.PhotoId);

        builder.Property(p => p.PhotoUrl)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(p => p.Caption)
            .HasMaxLength(200);

        builder.HasOne(p => p.Programme)
            .WithMany(prog => prog.Photos)
            .HasForeignKey(p => p.ProgrammeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(p => p.ProgrammeId);
    }
}
