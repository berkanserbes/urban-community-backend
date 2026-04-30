using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class CommunityPostComment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CommunityPostId { get; set; }
    public CommunityPost CommunityPost { get; set; } = null!;
}