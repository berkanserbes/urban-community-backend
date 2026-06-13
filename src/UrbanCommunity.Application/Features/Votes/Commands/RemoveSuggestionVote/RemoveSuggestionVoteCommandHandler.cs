using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Votes.Commands.RemoveSuggestionVote;

public sealed class RemoveSuggestionVoteCommandHandler(
    ISuggestionVoteService suggestionVoteService)
    : IRequestHandler<
        RemoveSuggestionVoteCommand,
        Result<RemoveSuggestionVoteCommandResponse>>
{
    public async Task<Result<RemoveSuggestionVoteCommandResponse>> Handle(
        RemoveSuggestionVoteCommand request,
        CancellationToken cancellationToken)
    {
        var mutation = await suggestionVoteService.RemoveVoteAsync(
            request.SuggestionId,
            request.UserId,
            cancellationToken);

        if (!mutation.IsSuccess)
        {
            return Result<RemoveSuggestionVoteCommandResponse>.Failure(
                [mutation.ErrorMessage!]);
        }

        return Result<RemoveSuggestionVoteCommandResponse>.Success(
            new RemoveSuggestionVoteCommandResponse(
                request.SuggestionId,
                mutation.UpvoteCount,
                mutation.DownvoteCount,
                mutation.VoteScore,
                "Oyunuz başarıyla kaldırıldı."));
    }
}
