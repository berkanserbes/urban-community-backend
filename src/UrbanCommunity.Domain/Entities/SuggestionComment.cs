using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class SuggestionComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid SuggestionId { get; set; }
    public Suggestion Suggestion { get; set; } = null!;
}