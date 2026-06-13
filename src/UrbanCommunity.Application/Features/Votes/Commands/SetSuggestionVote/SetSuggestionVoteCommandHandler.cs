using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Votes.Commands.SetSuggestionVote;

public sealed class SetSuggestionVoteCommandHandler(
    ISuggestionVoteService suggestionVoteService)
    : IRequestHandler<SetSuggestionVoteCommand, Result<SetSuggestionVoteCommandResponse>>
{
    public async Task<Result<SetSuggestionVoteCommandResponse>> Handle(
        SetSuggestionVoteCommand request,
        CancellationToken cancellationToken)
    {
        var mutation = await suggestionVoteService.SetVoteAsync(
            request.SuggestionId,
            request.UserId,
            request.IsUpvote,
            cancellationToken);

        if (!mutation.IsSuccess)
        {
            return Result<SetSuggestionVoteCommandResponse>.Failure(
                [mutation.ErrorMessage!]);
        }

        return Result<SetSuggestionVoteCommandResponse>.Success(
            new SetSuggestionVoteCommandResponse(
                request.SuggestionId,
                mutation.UpvoteCount,
                mutation.DownvoteCount,
                mutation.VoteScore,
                request.IsUpvote,
                request.IsUpvote
                    ? "Öneriyi beğendiniz."
                    : "Öneriyi beğenmediniz."));
    }
}
