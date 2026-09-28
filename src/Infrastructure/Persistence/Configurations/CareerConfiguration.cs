using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CareerConfiguration : IEntityTypeConfiguration<Career>
{
    public void Configure(EntityTypeBuilder<Career> builder)
    {
        builder.ToTable("careers");

        builder.HasKey(c => c.CareerId);

        builder.Property(c => c.PositionTitle)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(c => c.Department)
            .HasMaxLength(100);

        builder.Property(c => c.Description)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Requirements)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Responsibilities)
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Location)
            .HasMaxLength(100);

        builder.Property(c => c.EmploymentType)
            .HasMaxLength(50);

        builder.Property(c => c.SalaryRange)
            .HasMaxLength(100);

        builder.HasIndex(c => c.IsActive);
        builder.HasIndex(c => c.PostedDate);

        // Soft delete: hide deleted careers from normal queries.
        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
