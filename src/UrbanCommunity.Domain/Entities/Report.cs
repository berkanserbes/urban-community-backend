using NetTopologySuite.Geometries;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Report : BaseEntity
{
    private static readonly IReadOnlyDictionary<ReportStatus, ReportStatus[]> AllowedStatusTransitions =
        new Dictionary<ReportStatus, ReportStatus[]>
        {
            [ReportStatus.Pending] =
            [
                ReportStatus.AI_Review,
                ReportStatus.Confirmed,
                ReportStatus.Rejected
            ],
            [ReportStatus.AI_Review] =
            [
                ReportStatus.Confirmed,
                ReportStatus.Rejected
            ],
            [ReportStatus.Confirmed] =
            [
                ReportStatus.InProgress,
                ReportStatus.Resolved,
                ReportStatus.Rejected
            ],
            [ReportStatus.InProgress] =
            [
                ReportStatus.Resolved,
                ReportStatus.Rejected
            ],
            [ReportStatus.Resolved] =
            [ReportStatus.InProgress],
            [ReportStatus.Rejected] =
            [
                ReportStatus.Pending,
                ReportStatus.AI_Review
            ]
        };

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // PostGIS Coğrafi Nokta
    public Point Location { get; set; } = null!;
    
    public ReportStatus Status { get; set; } = ReportStatus.Pending;
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;

    // AI Benzerlik Hash (Mükerrer kontrolü için)
    public string? SimilarityHash { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid CategoryId { get; set; }
    public ReportCategory Category { get; set; } = null!;

    public Guid NeighborhoodId { get; set; }
    public Neighborhood Neighborhood { get; set; } = null!;
    
    public Guid? AssignedDepartmentId { get; set; }
    public Department? AssignedDepartment { get; set; }

    public ICollection<ReportMedia> Media { get; set; } = new List<ReportMedia>();
    public ICollection<ReportHistory> History { get; set; } = new List<ReportHistory>();
    public ICollection<ReportComment> Comments { get; set; } = new List<ReportComment>();

    public IReadOnlyCollection<ReportStatus> GetAllowedNextStatuses()
    {
        return AllowedStatusTransitions.TryGetValue(Status, out var allowedStatuses)
            ? allowedStatuses
            : Array.Empty<ReportStatus>();
    }

    public bool CanTransitionTo(ReportStatus newStatus)
    {
        return newStatus == Status || GetAllowedNextStatuses().Contains(newStatus);
    }

    public void ChangeStatus(ReportStatus newStatus, Guid? changedByUserId = null, string? note = null)
    {
        if (newStatus == Status)
        {
            return;
        }

        if (!CanTransitionTo(newStatus))
        {
            throw new DomainException($"Geçersiz durum geçişi: {Status} -> {newStatus}");
        }

        var previousStatus = Status;

        Status = newStatus;
        ResolvedAt = newStatus == ReportStatus.Resolved ? DateTime.UtcNow : null;
        UpdatedAt = DateTime.UtcNow;

        History.Add(new ReportHistory
        {
            ReportId = Id,
            FromStatus = previousStatus,
            ToStatus = newStatus,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            ChangedByUserId = changedByUserId
        });
    }

    public void AssignDepartment(Guid? departmentId)
    {
        AssignedDepartmentId = departmentId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePriority(ReportPriority priority)
    {
        Priority = priority;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetSimilarityHash(string? similarityHash)
    {
        SimilarityHash = string.IsNullOrWhiteSpace(similarityHash) ? null : similarityHash.Trim();
        UpdatedAt = DateTime.UtcNow;
    }
}
