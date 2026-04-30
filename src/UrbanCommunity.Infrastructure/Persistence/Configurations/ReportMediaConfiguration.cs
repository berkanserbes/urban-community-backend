using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class ReportMediaConfiguration : BaseEntityConfiguration<ReportMedia>
{
    public override void Configure(EntityTypeBuilder<ReportMedia> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_ReportMedias_DisplayOrder_NonNegative",
                "\"DisplayOrder\" >= 0");

            table.HasCheckConstraint(
                "CK_ReportMedias_FileSizeBytes_NonNegative",
                "\"FileSizeBytes\" IS NULL OR \"FileSizeBytes\" >= 0");
        });
        
        builder.Property(rm => rm.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(rm => rm.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(rm => rm.OriginalFileName)
            .HasMaxLength(255);

        builder.Property(rm => rm.ContentType)
            .HasMaxLength(100);

        builder.Property(rm => rm.DisplayOrder)
            .HasDefaultValue(0);

        builder.HasIndex(rm => new { rm.ReportId, rm.DisplayOrder });

        builder.HasOne(rm => rm.Report)
            .WithMany(r => r.Media)
            .HasForeignKey(rm => rm.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
