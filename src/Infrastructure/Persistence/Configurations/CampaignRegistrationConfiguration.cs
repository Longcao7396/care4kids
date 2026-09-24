using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CampaignRegistrationConfiguration : IEntityTypeConfiguration<CampaignRegistration>
{
    public void Configure(EntityTypeBuilder<CampaignRegistration> builder)
    {
        builder.ToTable("campaign_registrations");

        builder.HasKey(r => r.RegistrationId);

        builder.Property(r => r.Status)
            .HasMaxLength(20);

        builder.Property(r => r.Notes)
            .HasMaxLength(500);

        // Relationships
        builder.HasOne(r => r.Campaign)
            .WithMany(c => c.Registrations)
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.User)
            .WithMany(u => u.CampaignRegistrations)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique index to prevent duplicate registrations
        builder.HasIndex(r => new { r.CampaignId, r.UserId }).IsUnique();
    }
}
