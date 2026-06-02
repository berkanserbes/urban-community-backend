using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class CommunityPostConfiguration : BaseEntityConfiguration<CommunityPost>
{
    public override void Configure(EntityTypeBuilder<CommunityPost> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_CommunityPosts_Scope_Location",
                "(\"Scope\" = 1 AND \"ProvinceId\" IS NOT NULL AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR " +
                "(\"Scope\" = 2 AND \"DistrictId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR " +
                "(\"Scope\" = 3 AND \"NeighborhoodId\" IS NOT NULL AND \"ProvinceId\" IS NULL AND \"DistrictId\" IS NULL)");
        });
        
        builder.Property(cp => cp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cp => cp.Scope)
            .IsRequired()
            .HasDefaultValue(CommunityPostScope.Neighborhood);

        builder.HasOne(cp => cp.User)
            .WithMany(user => user.CommunityPosts)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.Province)
            .WithMany()
            .HasForeignKey(cp => cp.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.District)
            .WithMany()
            .HasForeignKey(cp => cp.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.Neighborhood)
            .WithMany(neighborhood => neighborhood.CommunityPosts)
            .HasForeignKey(cp => cp.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cp => cp.Status);
        builder.HasIndex(cp => cp.Type);
        builder.HasIndex(cp => new { cp.Scope, cp.ProvinceId, cp.Status });
        builder.HasIndex(cp => new { cp.Scope, cp.DistrictId, cp.Status });
        builder.HasIndex(cp => new { cp.Scope, cp.NeighborhoodId, cp.Status });
    }
}
