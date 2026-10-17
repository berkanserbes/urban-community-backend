namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncementStats;

public sealed record GetManageableAnnouncementStatsQueryResponse(
    int Total,
    int Active,
    int AllScope,
    int ExpiringSoon)
{
    public static readonly GetManageableAnnouncementStatsQueryResponse Empty = new(0, 0, 0, 0);
}
