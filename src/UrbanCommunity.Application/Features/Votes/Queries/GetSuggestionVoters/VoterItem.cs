namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed record VoterItem(
    Guid UserId,
    string FullName,
    DateTime VotedAt,
    bool IsUpvote);
