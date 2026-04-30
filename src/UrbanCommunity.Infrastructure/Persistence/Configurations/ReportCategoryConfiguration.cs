using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class ReportCategoryConfiguration : BaseEntityConfiguration<ReportCategory>
{
    public override void Configure(EntityTypeBuilder<ReportCategory> builder)
    {
        base.Configure(builder);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(c => c.Description)
            .HasMaxLength(250);

        builder.Property(c => c.IconUrl)
            .HasMaxLength(500);
    }
}
