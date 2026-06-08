using NetTopologySuite.Geometries;
using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Domain.Entities;

public class Report : BaseEntity
{
    private static readonly IReadOnlyDictionary<ReportStatus, ReportStatus[]> AllowedStatusTransitions =
        new Dictionary<ReportStatus, ReportStatus[]>
        {
            [ReportStatus.PendingReview] =
            [
                ReportStatus.AiReview,
                ReportStatus.Confirmed,
                ReportStatus.Rejected
            ],
            [ReportStatus.AiReview] =
            [
                ReportStatus.PendingReview
            ],
            [ReportStatus.Confirmed] =
            [
                ReportStatus.InProgress,
                ReportStatus.Rejected
            ],
            [ReportStatus.InProgress] =
            [
                ReportStatus.Confirmed,
                ReportStatus.Resolved,
                ReportStatus.Rejected
            ],
            [ReportStatus.Resolved] =
            [ReportStatus.InProgress],
            [ReportStatus.Rejected] =
            [
                ReportStatus.PendingReview,
                ReportStatus.AiReview
            ]
        };

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    
    // PostGIS Coğrafi Nokta
    public Point Location { get; set; } = null!;
    
    public ReportStatus Status { get; set; } = ReportStatus.AiReview;
    public ReportPriority Priority { get; set; } = ReportPriority.Medium;

    // Normalized title and description hash for exact duplicate detection.
    public string? ContentHash { get; set; }

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
    public ICollection<ReportSimilarityMatch> SimilarReportMatches { get; set; } = new List<ReportSimilarityMatch>();
    public ICollection<ReportSimilarityMatch> ReferencedAsSimilarMatches { get; set; } = new List<ReportSimilarityMatch>();

    public IReadOnlyCollection<ReportStatus> GetAllowedNextStatuses()
    {
        return AllowedStatusTransitions.TryGetValue(Status, out var allowedStatuses)
            ? allowedStatuses
            : Array.Empty<ReportStatus>();
    }

    public bool CanTransitionTo(ReportStatus newStatus)
    {
        return GetAllowedNextStatuses().Contains(newStatus);
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
    }

    public void UpdatePriority(ReportPriority priority)
    {
        Priority = priority;
    }

    public void SetContentHash(string? contentHash)
    {
        ContentHash = string.IsNullOrWhiteSpace(contentHash) ? null : contentHash.Trim();
    }
}
