namespace UrbanCommunity.Application.Features.Votes.Commands.RemoveSuggestionVote;

public sealed record RemoveSuggestionVoteCommandResponse(
    Guid SuggestionId,
    int UpvoteCount,
    int DownvoteCount,
    int VoteScore,
    string Message);
