using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Votes.Commands.RemoveSuggestionVote;

public sealed class RemoveSuggestionVoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<RemoveSuggestionVoteCommand, Result<RemoveSuggestionVoteCommandResponse>>
{
    public async Task<Result<RemoveSuggestionVoteCommandResponse>> Handle(RemoveSuggestionVoteCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions
            .FirstOrDefaultAsync(s => s.Id == request.SuggestionId, cancellationToken)
            ?? throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        var vote = await context.Votes
            .FirstOrDefaultAsync(v => v.SuggestionId == request.SuggestionId && v.UserId == request.UserId, cancellationToken);

        if (vote is null)
        {
            return Result<RemoveSuggestionVoteCommandResponse>.Failure(["Bu öneri için oyunuz bulunmuyor."]);
        }

        context.Votes.Remove(vote);

        var upvoteCount = await context.Votes.CountAsync(
            v => v.SuggestionId == request.SuggestionId && v.IsUpvote, cancellationToken);

        var downvoteCount = await context.Votes.CountAsync(
            v => v.SuggestionId == request.SuggestionId && !v.IsUpvote, cancellationToken);

        if (vote.IsUpvote)
            upvoteCount--;
        else
            downvoteCount--;

        suggestion.VoteScore = upvoteCount - downvoteCount;

        await context.SaveChangesAsync(cancellationToken);

        return Result<RemoveSuggestionVoteCommandResponse>.Success(
            new RemoveSuggestionVoteCommandResponse(
                request.SuggestionId,
                upvoteCount,
                downvoteCount,
                suggestion.VoteScore,
                "Oyunuz başarıyla kaldırıldı."));
    }
}
