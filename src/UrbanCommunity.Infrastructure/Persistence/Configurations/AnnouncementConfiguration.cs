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

            table.HasCheckConstraint(
                "CK_Announcements_Scope_Location",
                "(\"Scope\" = 0 AND \"ProvinceId\" IS NULL AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR " +
                "(\"Scope\" = 1 AND \"ProvinceId\" IS NOT NULL AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR " +
                "(\"Scope\" = 2 AND \"DistrictId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR " +
                "(\"Scope\" = 3 AND \"NeighborhoodId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"DistrictId\" IS NULL)");
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

        builder.HasOne(a => a.Province)
            .WithMany()
            .HasForeignKey(a => a.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.District)
            .WithMany()
            .HasForeignKey(a => a.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Neighborhood)
            .WithMany(neighborhood => neighborhood.Announcements)
            .HasForeignKey(a => a.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(a => a.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(a => new { a.Scope, a.ProvinceId, a.IsActive });
        builder.HasIndex(a => new { a.Scope, a.DistrictId, a.IsActive });
        builder.HasIndex(a => new { a.Scope, a.NeighborhoodId, a.IsActive });
        builder.HasIndex(a => a.ExpiresAt);
    }
}
