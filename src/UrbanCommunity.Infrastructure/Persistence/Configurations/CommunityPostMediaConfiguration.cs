using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class CommunityPostMediaConfiguration : BaseEntityConfiguration<CommunityPostMedia>
{
    public override void Configure(EntityTypeBuilder<CommunityPostMedia> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_CommunityPostMedias_DisplayOrder_NonNegative",
                "\"DisplayOrder\" >= 0");

            table.HasCheckConstraint(
                "CK_CommunityPostMedias_FileSizeBytes_NonNegative",
                "\"FileSizeBytes\" IS NULL OR \"FileSizeBytes\" >= 0");
        });

        builder.Property(m => m.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.ThumbnailUrl)
            .HasMaxLength(500);

        builder.Property(m => m.OriginalFileName)
            .HasMaxLength(255);

        builder.Property(m => m.ContentType)
            .HasMaxLength(100);

        builder.Property(m => m.DisplayOrder)
            .HasDefaultValue(0);

        builder.HasIndex(m => new { m.CommunityPostId, m.DisplayOrder });

        builder.HasOne(m => m.CommunityPost)
            .WithMany(cp => cp.Media)
            .HasForeignKey(m => m.CommunityPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
