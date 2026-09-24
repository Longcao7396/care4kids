using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CmsPageConfiguration : IEntityTypeConfiguration<CmsPage>
{
    public void Configure(EntityTypeBuilder<CmsPage> builder)
    {
        builder.ToTable("cms_pages");

        builder.HasKey(p => p.PageId);

        builder.Property(p => p.PageKey)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.PageSlug)
            .HasMaxLength(100);

        builder.Property(p => p.PageTitle)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Content)
            .HasColumnType("nvarchar(max)");

        builder.Property(p => p.MetaDescription)
            .HasMaxLength(255);

        builder.Property(p => p.MetaKeywords)
            .HasMaxLength(255);

        builder.HasIndex(p => p.PageKey).IsUnique();
        builder.HasIndex(p => p.PageSlug);
    }
}
