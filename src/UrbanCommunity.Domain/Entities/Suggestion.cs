using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Suggestion : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    
    public SuggestionStatus Status { get; set; } = SuggestionStatus.Active;
    public int VoteCount { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
