namespace UrbanCommunity.Application.Events;

public sealed record ReportAiReviewRequestedEvent
{
    public Guid ReportId { get; init; }
    public bool IsInitialCategorization { get; init; }
    public DateTime RequestedAt { get; init; }
}
