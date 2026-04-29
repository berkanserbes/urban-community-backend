using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class Vote : BaseEntity
{
    public Guid SuggestionId { get; set; }
    public Suggestion Suggestion { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // true = Upvote (+1), false = Downvote (-1)
    public bool IsUpvote { get; set; }
}
