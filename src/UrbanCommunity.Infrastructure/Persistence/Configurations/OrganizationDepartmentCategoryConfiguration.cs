using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class OrganizationDepartmentCategoryConfiguration
    : IEntityTypeConfiguration<OrganizationDepartmentCategory>
{
    public void Configure(EntityTypeBuilder<OrganizationDepartmentCategory> builder)
    {
        builder.HasKey(item => new { item.OrganizationDepartmentId, item.ReportCategoryId });

        builder.HasOne(item => item.OrganizationDepartment)
            .WithMany(department => department.Categories)
            .HasForeignKey(item => item.OrganizationDepartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(item => item.ReportCategory)
            .WithMany(category => category.OrganizationDepartments)
            .HasForeignKey(item => item.ReportCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => item.ReportCategoryId);
    }
}
