using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("organizations");

        builder.HasKey(o => o.OrganizationId);

        builder.Property(o => o.OrganizationName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(o => o.OrganizationType)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(o => o.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(o => o.LogoUrl)
            .HasMaxLength(255);

        builder.Property(o => o.WebsiteUrl)
            .HasMaxLength(200);

        builder.Property(o => o.ContactEmail)
            .HasMaxLength(100);

        builder.Property(o => o.ContactPhone)
            .HasMaxLength(20);

        builder.Property(o => o.Address)
            .HasMaxLength(255);

        builder.Property(o => o.RegistrationNumber)
            .HasMaxLength(50);

        builder.Property(o => o.Mission)
            .HasMaxLength(500);

        builder.Property(o => o.Vision)
            .HasMaxLength(500);

        builder.Property(o => o.ContributionAmount)
            .HasPrecision(18, 2);

        builder.Property(o => o.ContributionType)
            .HasMaxLength(50);

        builder.HasIndex(o => o.IsFeatured);
        builder.HasIndex(o => o.DisplayOrder);
    }
}
