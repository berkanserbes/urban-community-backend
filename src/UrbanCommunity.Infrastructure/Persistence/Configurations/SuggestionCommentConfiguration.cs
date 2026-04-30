using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class SuggestionCommentConfiguration : BaseEntityConfiguration<SuggestionComment>
{
    public override void Configure(EntityTypeBuilder<SuggestionComment> builder)
    {
        base.Configure(builder);

        builder.Property(comment => comment.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(comment => comment.User)
            .WithMany(user => user.SuggestionComments)
            .HasForeignKey(comment => comment.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(comment => comment.Suggestion)
            .WithMany(suggestion => suggestion.Comments)
            .HasForeignKey(comment => comment.SuggestionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}