using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : BaseEntityConfiguration<Report>
{
    public override void Configure(EntityTypeBuilder<Report> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Reports_ResolvedAt_Matches_Status",
                $"(\"Status\" = {(int)ReportStatus.Resolved} AND \"ResolvedAt\" IS NOT NULL) OR (\"Status\" <> {(int)ReportStatus.Resolved} AND \"ResolvedAt\" IS NULL)");
        });

        builder.Property(r => r.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(r => r.SimilarityHash)
            .HasMaxLength(256);

        // İlişkiler
        builder.HasOne(r => r.User)
            .WithMany(user => user.Reports)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Category)
            .WithMany(c => c.Reports)
            .HasForeignKey(r => r.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Neighborhood)
            .WithMany(neighborhood => neighborhood.Reports)
            .HasForeignKey(r => r.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.AssignedDepartment)
            .WithMany(d => d.AssignedReports)
            .HasForeignKey(r => r.AssignedDepartmentId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.Status);
        builder.HasIndex(r => r.Priority);
        builder.HasIndex(r => new { r.NeighborhoodId, r.Status });

        // PostGIS Spatial Index (konum bazlı sorguları hızlandırır)
        builder.HasIndex(r => r.Location)
            .HasMethod("GIST");
    }
}
