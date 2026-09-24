using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(i => i.InvitationId);

        builder.Property(i => i.InviteeName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(i => i.InviteeEmail)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(i => i.PersonalMessage)
            .HasMaxLength(500);

        builder.Property(i => i.Status)
            .HasMaxLength(20);

        builder.Property(i => i.InvitationToken)
            .HasMaxLength(64);

        builder.Property(i => i.FailureReason)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(i => i.Inviter)
            .WithMany(u => u.SentInvitations)
            .HasForeignKey(i => i.InviterUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(i => i.InvitationToken);
        builder.HasIndex(i => i.Status);
    }
}
