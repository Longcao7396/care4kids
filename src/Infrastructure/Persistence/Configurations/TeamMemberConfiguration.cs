using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class TeamMemberConfiguration : IEntityTypeConfiguration<TeamMember>
{
    public void Configure(EntityTypeBuilder<TeamMember> builder)
    {
        builder.ToTable("team_members");

        builder.HasKey(t => t.TeamMemberId);

        builder.Property(t => t.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(t => t.RoleTitle)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(t => t.Department)
            .HasMaxLength(100);

        builder.Property(t => t.Bio)
            .HasColumnType("nvarchar(max)");

        builder.Property(t => t.PhotoUrl)
            .HasMaxLength(500);

        builder.Property(t => t.Email)
            .HasMaxLength(100);

        builder.Property(t => t.LinkedInUrl)
            .HasMaxLength(255);

        builder.Property(t => t.TwitterUrl)
            .HasMaxLength(255);

        builder.Property(t => t.FacebookUrl)
            .HasMaxLength(255);

        builder.HasOne(t => t.CreatedByUser)
            .WithMany(u => u.CreatedTeamMembers)
            .HasForeignKey(t => t.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(t => t.IsFeatured);
        builder.HasIndex(t => t.DisplayOrder);

        // Soft delete: hide deleted team members from normal queries.
        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}
