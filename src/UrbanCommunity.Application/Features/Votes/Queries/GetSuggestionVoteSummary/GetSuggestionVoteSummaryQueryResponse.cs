namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoteSummary;

public sealed record GetSuggestionVoteSummaryQueryResponse(
    Guid SuggestionId,
    int UpvoteCount,
    int DownvoteCount,
    int VoteScore,
    bool? CurrentUserIsUpvote);
