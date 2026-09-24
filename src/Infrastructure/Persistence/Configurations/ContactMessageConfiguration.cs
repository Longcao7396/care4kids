using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.ToTable("contact_messages");

        builder.HasKey(c => c.ContactId);

        builder.Property(c => c.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasMaxLength(20);

        builder.Property(c => c.Subject)
            .HasMaxLength(200);

        builder.Property(c => c.Message)
            .IsRequired();

        builder.Property(c => c.ReplyMessage)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(c => c.IsRead);
    }
}
