using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class ReportCommentConfiguration : BaseEntityConfiguration<ReportComment>
{
    public override void Configure(EntityTypeBuilder<ReportComment> builder)
    {
        base.Configure(builder);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(comment => comment.User)
            .WithMany(user => user.ReportComments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(comment => comment.Report)
            .WithMany(report => report.Comments)
            .HasForeignKey(comment => comment.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        // Yorumlar ait oldukları kayda göre CreatedAt sırasıyla sayfalanır
        builder.HasIndex(comment => new { comment.ReportId, comment.CreatedAt });
    }
}