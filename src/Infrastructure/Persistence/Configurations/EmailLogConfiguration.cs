using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class EmailLogConfiguration : IEntityTypeConfiguration<EmailLog>
{
    public void Configure(EntityTypeBuilder<EmailLog> builder)
    {
        builder.ToTable("email_logs");

        builder.HasKey(e => e.EmailLogId);

        builder.Property(e => e.ToEmail)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(e => e.Subject)
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Body)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.Status)
            .HasMaxLength(20);

        builder.Property(e => e.ErrorMessage)
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.EmailType)
            .HasMaxLength(50);

        builder.HasIndex(e => e.ToEmail);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.SentAt);
    }
}
