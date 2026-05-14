namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed record GetSuggestionVotersQueryResponse(
    int UpvoteCount,
    int DownvoteCount,
    IReadOnlyList<VoterItem> Upvoters,
    IReadOnlyList<VoterItem> Downvoters);

public sealed record VoterItem(
    Guid UserId,
    string FullName,
    DateTime VotedAt);
