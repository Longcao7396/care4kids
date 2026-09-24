using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CareerApplicationConfiguration : IEntityTypeConfiguration<CareerApplication>
{
    public void Configure(EntityTypeBuilder<CareerApplication> builder)
    {
        builder.ToTable("career_applications");

        builder.HasKey(a => a.ApplicationId);

        builder.Property(a => a.ApplicantName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.Phone)
            .HasMaxLength(20);

        builder.Property(a => a.ResumeUrl)
            .HasMaxLength(255);

        builder.Property(a => a.CoverLetter)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.LinkedInUrl)
            .HasMaxLength(200);

        builder.Property(a => a.PortfolioUrl)
            .HasMaxLength(200);

        builder.Property(a => a.Status)
            .HasMaxLength(20);

        builder.Property(a => a.Notes)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(a => a.Career)
            .WithMany(c => c.Applications)
            .HasForeignKey(a => a.CareerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.CareerId);
        builder.HasIndex(a => a.Status);
    }
}
