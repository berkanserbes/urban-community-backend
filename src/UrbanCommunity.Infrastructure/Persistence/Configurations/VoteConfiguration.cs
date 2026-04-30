using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class VoteConfiguration : BaseEntityConfiguration<Vote>
{
    public override void Configure(EntityTypeBuilder<Vote> builder)
    {
        base.Configure(builder);
        
        // Bir kullanıcı bir öneriye sadece bir kez oy verebilir
        builder.HasIndex(v => new { v.UserId, v.SuggestionId }).IsUnique();

        builder.HasOne(v => v.Suggestion)
            .WithMany(s => s.Votes)
            .HasForeignKey(v => v.SuggestionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(v => v.User)
            .WithMany(user => user.Votes)
            .HasForeignKey(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
