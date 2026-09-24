using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        builder.ToTable("conversations");

        builder.HasKey(c => c.ConversationId);

        builder.Property(c => c.Subject)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.ConversationType)
            .HasMaxLength(50);

        builder.Property(c => c.Status)
            .HasMaxLength(20);

        builder.Property(c => c.Priority)
            .HasMaxLength(20);

        builder.HasOne(c => c.User)
            .WithMany(u => u.Conversations)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.Status);
    }
}
