namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionStats;

public sealed record GetSuggestionStatsQueryResponse(
    int Total,
    int Active,
    int Reviewing,
    int Implementation,
    int Closed)
{
    public static readonly GetSuggestionStatsQueryResponse Empty = new(0, 0, 0, 0, 0);
}
