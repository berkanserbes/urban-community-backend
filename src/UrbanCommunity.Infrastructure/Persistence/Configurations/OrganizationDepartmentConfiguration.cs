using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class OrganizationDepartmentConfiguration : BaseEntityConfiguration<OrganizationDepartment>
{
    public override void Configure(EntityTypeBuilder<OrganizationDepartment> builder)
    {
        base.Configure(builder);

        builder.Property(item => item.DisplayName).HasMaxLength(150);
        builder.Property(item => item.ContactEmail).HasMaxLength(100);
        builder.Property(item => item.ContactPhone).HasMaxLength(20);
        builder.Property(item => item.IsActive).HasDefaultValue(true);

        builder.HasOne(item => item.Organization)
            .WithMany(organization => organization.Departments)
            .HasForeignKey(item => item.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(item => item.Department)
            .WithMany(department => department.OrganizationDepartments)
            .HasForeignKey(item => item.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(item => new { item.OrganizationId, item.DepartmentId })
            .IsUnique();
    }
}
