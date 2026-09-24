using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class FaqConfiguration : IEntityTypeConfiguration<Faq>
{
    public void Configure(EntityTypeBuilder<Faq> builder)
    {
        builder.ToTable("faqs");

        builder.HasKey(f => f.FaqId);

        builder.Property(f => f.Question)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(f => f.Answer)
            .IsRequired();

        builder.Property(f => f.Category)
            .HasMaxLength(100);

        builder.HasOne(f => f.CreatedByUser)
            .WithMany(u => u.CreatedFaqs)
            .HasForeignKey(f => f.CreatedBy)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(f => f.IsActive);
        builder.HasIndex(f => f.IsFeatured);
        builder.HasIndex(f => f.Category);
    }
}
