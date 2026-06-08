using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public sealed class ReportSimilarityMatchConfiguration : BaseEntityConfiguration<ReportSimilarityMatch>
{
    public override void Configure(EntityTypeBuilder<ReportSimilarityMatch> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_ReportSimilarityMatches_DifferentReports",
                "\"ReportId\" <> \"SimilarReportId\"");
            table.HasCheckConstraint(
                "CK_ReportSimilarityMatches_Score_Range",
                "\"SimilarityScore\" >= 0 AND \"SimilarityScore\" <= 1");
        });

        builder.Property(match => match.Reasoning)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(match => match.Report)
            .WithMany(report => report.SimilarReportMatches)
            .HasForeignKey(match => match.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(match => match.SimilarReport)
            .WithMany(report => report.ReferencedAsSimilarMatches)
            .HasForeignKey(match => match.SimilarReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(match => new { match.ReportId, match.SimilarReportId })
            .IsUnique();
        builder.HasIndex(match => match.SimilarReportId);
    }
}
