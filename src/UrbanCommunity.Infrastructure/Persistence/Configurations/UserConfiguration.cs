using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_AspNetUsers_OfficialAccess",
                $"(" +
                $"\"Role\" = {(int)UserRole.Official} AND " +
                "\"DepartmentId\" IS NOT NULL AND " +
                "((\"OfficialJurisdictionLevel\" = 1 AND \"OfficialProvinceId\" IS NOT NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NULL) OR " +
                "(\"OfficialJurisdictionLevel\" = 2 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NOT NULL AND \"OfficialNeighborhoodId\" IS NULL) OR " +
                "(\"OfficialJurisdictionLevel\" = 3 AND \"OfficialProvinceId\" IS NULL AND \"OfficialDistrictId\" IS NULL AND \"OfficialNeighborhoodId\" IS NOT NULL))" +
                ") OR (" +
                $"\"Role\" <> {(int)UserRole.Official} AND " +
                "\"DepartmentId\" IS NULL AND " +
                "\"OfficialJurisdictionLevel\" IS NULL AND " +
                "\"OfficialProvinceId\" IS NULL AND " +
                "\"OfficialDistrictId\" IS NULL AND " +
                "\"OfficialNeighborhoodId\" IS NULL" +
                ")");
        });

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(u => u.Neighborhood)
            .WithMany(n => n.Users)
            .HasForeignKey(u => u.NeighborhoodId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(u => u.Department)
            .WithMany(d => d.Officials)
            .HasForeignKey(u => u.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.OfficialProvince)
            .WithMany()
            .HasForeignKey(u => u.OfficialProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.OfficialDistrict)
            .WithMany()
            .HasForeignKey(u => u.OfficialDistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.OfficialNeighborhood)
            .WithMany()
            .HasForeignKey(u => u.OfficialNeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(u => u.ProfileImageUrl)
            .HasMaxLength(500);

        builder.Property(u => u.Bio)
            .HasMaxLength(300);

        builder.Property(u => u.IsActive)
            .HasDefaultValue(true);

        builder.Property(u => u.CreatedAt)
            .IsRequired();

        builder.Property(u => u.UpdatedAt);

        builder.HasIndex(u => u.Role);
        builder.HasIndex(u => u.IsActive);
        builder.HasIndex(u => u.DepartmentId);
        builder.HasIndex(u => u.NeighborhoodId);
        builder.HasIndex(u => new { u.OfficialJurisdictionLevel, u.OfficialProvinceId });
        builder.HasIndex(u => new { u.OfficialJurisdictionLevel, u.OfficialDistrictId });
        builder.HasIndex(u => new { u.OfficialJurisdictionLevel, u.OfficialNeighborhoodId });
    }
}
