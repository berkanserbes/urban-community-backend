using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class CommunityPostConfiguration : BaseEntityConfiguration<CommunityPost>
{
    public override void Configure(EntityTypeBuilder<CommunityPost> builder)
    {
        base.Configure(builder);
        
        builder.Property(cp => cp.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(cp => cp.Content)
            .IsRequired()
            .HasMaxLength(1500);

        builder.HasOne(cp => cp.User)
            .WithMany(user => user.CommunityPosts)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(cp => cp.Neighborhood)
            .WithMany(neighborhood => neighborhood.CommunityPosts)
            .HasForeignKey(cp => cp.NeighborhoodId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(cp => cp.Status);
        builder.HasIndex(cp => cp.Type);
    }
}
