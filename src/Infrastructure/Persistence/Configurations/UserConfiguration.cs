using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.Username)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Phone)
            .HasMaxLength(20);

        builder.Property(u => u.Address)
            .HasMaxLength(255);

        builder.Property(u => u.Profession)
            .HasMaxLength(100);

        builder.Property(u => u.Gender)
            .HasMaxLength(10);

        builder.Property(u => u.Role)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(u => u.VerificationToken)
            .HasMaxLength(100);

        // Indexes
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.Username).IsUnique();

        // Soft delete: hide deleted users from normal queries.
        builder.HasQueryFilter(u => !u.IsDeleted);
    }
}
