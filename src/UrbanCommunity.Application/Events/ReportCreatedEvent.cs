using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Events;

public sealed record ReportCreatedEvent
{
    public Guid ReportId { get; init; }
    public string ReportTitle { get; init; } = string.Empty;
    public Guid CreatedByUserId { get; init; }
    public Guid NeighborhoodId { get; init; }
    public Guid CategoryId { get; init; }
    public string CategoryName { get; init; } = string.Empty;
    public bool CategoryAssignedByAi { get; init; }
    public double? AiCategoryConfidence { get; init; }
    public ReportPriority Priority { get; init; }
    public bool PriorityAssignedByAi { get; init; }
    public double? AiPriorityScore { get; init; }
    public DateTime CreatedAt { get; init; }
}
