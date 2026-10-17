namespace UrbanCommunity.Application.Features.Users.Queries.GetUserStats;

public sealed record GetUserStatsQueryResponse(
    int Total,
    int Active,
    int Inactive,
    int Admin,
    int Official,
    int Citizen)
{
    public static readonly GetUserStatsQueryResponse Empty = new(0, 0, 0, 0, 0, 0);
}
