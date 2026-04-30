using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class AnnouncementConfiguration : BaseEntityConfiguration<Announcement>
{
    public override void Configure(EntityTypeBuilder<Announcement> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Announcements_ExpiresAt_After_CreatedAt",
                "\"ExpiresAt\" IS NULL OR \"ExpiresAt\" > \"CreatedAt\"");
        });

        builder.Property(a => a.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasOne(a => a.User)
            .WithMany(user => user.Announcements)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Neighborhood)
            .WithMany(neighborhood => neighborhood.Announcements)
            .HasForeignKey(a => a.NeighborhoodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(a => new { a.NeighborhoodId, a.IsActive });
        builder.HasIndex(a => a.ExpiresAt);
    }
}
