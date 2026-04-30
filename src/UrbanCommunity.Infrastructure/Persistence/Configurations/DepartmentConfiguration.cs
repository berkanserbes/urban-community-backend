using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class DepartmentConfiguration : BaseEntityConfiguration<Department>
{
    public override void Configure(EntityTypeBuilder<Department> builder)
    {
        base.Configure(builder);

        builder.Property(d => d.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(d => d.Description)
            .HasMaxLength(500);

        builder.Property(d => d.ContactEmail)
            .HasMaxLength(100);

        builder.Property(d => d.ContactPhone)
            .HasMaxLength(20);

        builder.HasIndex(d => d.Name)
            .IsUnique();

        builder.Navigation(d => d.Officials)
            .AutoInclude(false);
    }
}
