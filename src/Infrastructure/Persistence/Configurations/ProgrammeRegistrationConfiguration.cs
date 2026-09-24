using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class ProgrammeRegistrationConfiguration : IEntityTypeConfiguration<ProgrammeRegistration>
{
    public void Configure(EntityTypeBuilder<ProgrammeRegistration> builder)
    {
        builder.ToTable("programme_registrations");

        builder.HasKey(r => r.RegistrationId);

        builder.Property(r => r.Status)
            .HasMaxLength(20);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        builder.HasOne(r => r.Programme)
            .WithMany(p => p.Registrations)
            .HasForeignKey(r => r.ProgrammeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.ProgrammeRegistrations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => new { r.ProgrammeId, r.UserId }).IsUnique();
    }
}
