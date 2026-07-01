using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class OrganizationConfiguration : BaseEntityConfiguration<Organization>
{
    public override void Configure(EntityTypeBuilder<Organization> builder)
    {
        base.Configure(builder);

        builder.ToTable(table =>
        {
            table.HasCheckConstraint(
                "CK_Organizations_Jurisdiction",
                $"(\"JurisdictionLevel\" = {(int)OrganizationJurisdictionLevel.Province} AND \"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL) OR " +
                $"(\"JurisdictionLevel\" = {(int)OrganizationJurisdictionLevel.District} AND \"DistrictId\" IS NOT NULL AND \"NeighborhoodId\" IS NULL) OR " +
                $"(\"JurisdictionLevel\" = {(int)OrganizationJurisdictionLevel.Neighborhood} AND \"DistrictId\" IS NOT NULL AND \"NeighborhoodId\" IS NOT NULL)");
        });

        builder.Property(organization => organization.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(organization => organization.IsActive)
            .HasDefaultValue(true);

        builder.HasOne(organization => organization.Province)
            .WithMany()
            .HasForeignKey(organization => organization.ProvinceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(organization => organization.District)
            .WithMany()
            .HasForeignKey(organization => organization.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(organization => organization.Neighborhood)
            .WithMany()
            .HasForeignKey(organization => organization.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(organization => new
            {
                organization.Name,
                organization.JurisdictionLevel,
                organization.ProvinceId
            })
            .HasDatabaseName("UX_Organizations_ProvinceScope")
            .HasFilter("\"DistrictId\" IS NULL AND \"NeighborhoodId\" IS NULL")
            .IsUnique();

        builder.HasIndex(organization => new
            {
                organization.Name,
                organization.JurisdictionLevel,
                organization.ProvinceId,
                organization.DistrictId
            })
            .HasDatabaseName("UX_Organizations_DistrictScope")
            .HasFilter("\"DistrictId\" IS NOT NULL AND \"NeighborhoodId\" IS NULL")
            .IsUnique();

        builder.HasIndex(organization => new
            {
                organization.Name,
                organization.JurisdictionLevel,
                organization.ProvinceId,
                organization.DistrictId,
                organization.NeighborhoodId
            })
            .HasDatabaseName("UX_Organizations_NeighborhoodScope")
            .HasFilter("\"NeighborhoodId\" IS NOT NULL")
            .IsUnique();
    }
}
