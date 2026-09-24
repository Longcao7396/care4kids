using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.ToTable("password_reset_tokens");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Token)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.RequestIpAddress)
            .HasMaxLength(45);

        builder.Property(e => e.RequestUserAgent)
            .HasMaxLength(500);

        // Foreign key to users table
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for fast lookups
        // Index on Token for O(1) lookup when validating a reset token
        builder.HasIndex(e => e.Token);
        // Index on UserId + ExpiresAt for cleanup queries
        builder.HasIndex(e => new { e.UserId, e.ExpiresAt });
    }
}
