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
    public CommunityPostScope Scope { get; set; } = CommunityPostScope.Neighborhood;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? ProvinceId { get; set; }
    public Province? Province { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public Guid? NeighborhoodId { get; set; }
    public Neighborhood? Neighborhood { get; set; }

    public ICollection<CommunityPostMedia> Media { get; set; } = new List<CommunityPostMedia>();
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
            throw new DomainException($"Geçersiz durum geçişi: {Status} -> {newStatus}");
        }

        Status = newStatus;
    }
}
