namespace UrbanCommunity.Application.Interfaces;

public interface ISuggestionVoteService
{
    Task<SuggestionVoteMutationResult> SetVoteAsync(
        Guid suggestionId,
        Guid userId,
        bool isUpvote,
        CancellationToken cancellationToken);

    Task<SuggestionVoteMutationResult> RemoveVoteAsync(
        Guid suggestionId,
        Guid userId,
        CancellationToken cancellationToken);
}

public sealed record SuggestionVoteMutationResult(
    bool IsSuccess,
    string? ErrorMessage,
    int UpvoteCount,
    int DownvoteCount,
    int VoteScore,
    bool? CurrentUserIsUpvote);
