using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Events;

public sealed record ReportStatusChangedEvent
{
    public Guid ReportId { get; init; }
    public string ReportTitle { get; init; } = string.Empty;
    public Guid ReportOwnerId { get; init; }
    public ReportStatus OldStatus { get; init; }
    public ReportStatus NewStatus { get; init; }
    public string? Note { get; init; }
    public DateTime ChangedAt { get; init; }
}
