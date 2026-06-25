using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Infrastructure.Persistence.Configurations;

public class SuggestionConfiguration : BaseEntityConfiguration<Suggestion>
{
    public override void Configure(EntityTypeBuilder<Suggestion> builder)
    {
        base.Configure(builder);
        
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.Content)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(s => s.OfficialResponse)
            .HasMaxLength(2000);

        builder.Property(s => s.VoteScore)
            .HasDefaultValue(0);

        builder.HasOne(s => s.User)
            .WithMany(user => user.Suggestions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => s.Status);
    }
}
