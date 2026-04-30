using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class CommunityPost : BaseEntity
{
    private static readonly IReadOnlyDictionary<CommunityPostStatus, CommunityPostStatus[]> AllowedStatusTransitions =
        new Dictionary<CommunityPostStatus, CommunityPostStatus[]>
        {
            [CommunityPostStatus.Active] =
            [
                CommunityPostStatus.Reserved,
                CommunityPostStatus.Resolved,
                CommunityPostStatus.Expired,
                CommunityPostStatus.Archived
            ],
            [CommunityPostStatus.Reserved] =
            [
                CommunityPostStatus.Active,
                CommunityPostStatus.Resolved,
                CommunityPostStatus.Expired,
                CommunityPostStatus.Archived
            ],
            [CommunityPostStatus.Resolved] =
            [CommunityPostStatus.Archived],
            [CommunityPostStatus.Expired] =
            [CommunityPostStatus.Archived],
            [CommunityPostStatus.Archived] =
            []
        };

    public PostType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    public CommunityPostStatus Status { get; set; } = CommunityPostStatus.Active;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid NeighborhoodId { get; set; }
    public Neighborhood Neighborhood { get; set; } = null!;

    public ICollection<CommunityPostComment> Comments { get; set; } = new List<CommunityPostComment>();

    public IReadOnlyCollection<CommunityPostStatus> GetAllowedNextStatuses()
    {
        return AllowedStatusTransitions.TryGetValue(Status, out var allowedStatuses)
            ? allowedStatuses
            : Array.Empty<CommunityPostStatus>();
    }

    public bool CanTransitionTo(CommunityPostStatus newStatus)
    {
        return newStatus == Status || GetAllowedNextStatuses().Contains(newStatus);
    }

    public void ChangeStatus(CommunityPostStatus newStatus)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidOperationException($"Invalid community post status transition: {Status} -> {newStatus}");
        }

        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
    }
}
