using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class ReportHistoryConfiguration : BaseEntityConfiguration<ReportHistory>
{
    public override void Configure(EntityTypeBuilder<ReportHistory> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_ReportHistories_StatusChange_IsRequired",
                "\"FromStatus\" <> \"ToStatus\"");
        });
        
        builder.Property(rh => rh.Note)
            .HasMaxLength(500);

        builder.HasOne(rh => rh.Report)
            .WithMany(r => r.History)
            .HasForeignKey(rh => rh.ReportId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(rh => rh.ChangedByUser)
            .WithMany(user => user.ChangedReportHistories)
            .HasForeignKey(rh => rh.ChangedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(rh => new { rh.ReportId, rh.CreatedAt });
    }
}
