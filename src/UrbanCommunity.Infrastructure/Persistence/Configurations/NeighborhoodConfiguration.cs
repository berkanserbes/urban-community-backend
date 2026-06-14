using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class NeighborhoodConfiguration : BaseEntityConfiguration<Neighborhood>
{
    public override void Configure(EntityTypeBuilder<Neighborhood> builder)
    {
        base.Configure(builder);

        builder.Property(n => n.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.District)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(n => n.City)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(n => n.PolygonData)
            .HasColumnType("geometry")
            .IsRequired(false);

        builder.HasOne(n => n.DistrictNavigation)
            .WithMany(d => d.Neighborhoods)
            .HasForeignKey(n => n.DistrictId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasIndex(n => new { n.City, n.District, n.Name })
            .IsUnique();
    }
}
