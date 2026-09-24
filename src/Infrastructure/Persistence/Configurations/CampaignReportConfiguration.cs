using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GiveAID.Domain.Entities;

namespace GiveAID.Infrastructure.Persistence.Configurations;

public class CampaignReportConfiguration : IEntityTypeConfiguration<CampaignReport>
{
    public void Configure(EntityTypeBuilder<CampaignReport> builder)
    {
        builder.ToTable("campaign_reports");

        builder.HasKey(r => r.ReportId);

        builder.Property(r => r.TotalReceived)
            .HasPrecision(18, 2);

        builder.Property(r => r.TotalSpent)
            .HasPrecision(18, 2);

        builder.Property(r => r.ReportTitle)
            .HasMaxLength(200);

        builder.Property(r => r.ReportContent)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.ExpenseBreakdown)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Photos)
            .HasColumnType("nvarchar(max)");

        builder.Property(r => r.Documents)
            .HasColumnType("nvarchar(max)");

        builder.HasOne(r => r.Campaign)
            .WithMany(c => c.Reports)
            .HasForeignKey(r => r.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.CampaignId);
    }
}
