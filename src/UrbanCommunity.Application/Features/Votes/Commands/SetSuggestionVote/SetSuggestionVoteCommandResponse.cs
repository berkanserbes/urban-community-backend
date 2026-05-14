namespace UrbanCommunity.Application.Features.Votes.Commands.SetSuggestionVote;

public sealed record SetSuggestionVoteCommandResponse(
    Guid SuggestionId,
    int UpvoteCount,
    int DownvoteCount,
    int VoteScore,
    bool CurrentUserIsUpvote,
    string Message);
