namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReportStats;

public sealed record GetManageableReportStatsQueryResponse(
    int Total,
    int PendingReview,
    int AiReview,
    int Confirmed,
    int InProgress,
    int Resolved,
    int Rejected,
    int Urgent,
    int Unassigned,
    int NeedsReview)
{
    public static readonly GetManageableReportStatsQueryResponse Empty = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}
