using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class CommunityPostCommentConfiguration : BaseEntityConfiguration<CommunityPostComment>
{
    public override void Configure(EntityTypeBuilder<CommunityPostComment> builder)
    {
        base.Configure(builder);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(comment => comment.User)
            .WithMany(user => user.CommunityPostComments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(comment => comment.CommunityPost)
            .WithMany(post => post.Comments)
            .HasForeignKey(comment => comment.CommunityPostId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}