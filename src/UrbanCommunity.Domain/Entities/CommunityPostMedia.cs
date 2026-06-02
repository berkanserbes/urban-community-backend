using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class CommunityPostMedia : BaseEntity
{
    public Guid CommunityPostId { get; set; }
    public CommunityPost CommunityPost { get; set; } = null!;

    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? OriginalFileName { get; set; }
    public string? ContentType { get; set; }
    public long? FileSizeBytes { get; set; }
    public int DisplayOrder { get; set; }
    public MediaType MediaType { get; set; }
}
