using UrbanCommunity.Domain.Common;

namespace UrbanCommunity.Domain.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    // Hangi nesneye yorum yapıldığına dair opsiyonel alanlar
    public Guid? ReportId { get; set; }
    public Report? Report { get; set; }

    public Guid? SuggestionId { get; set; }
    public Suggestion? Suggestion { get; set; }

    public Guid? CommunityPostId { get; set; }
    public CommunityPost? CommunityPost { get; set; }
}
