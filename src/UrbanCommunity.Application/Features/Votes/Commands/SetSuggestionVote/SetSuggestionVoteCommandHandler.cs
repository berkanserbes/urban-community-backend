using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Votes.Commands.SetSuggestionVote;

public sealed class SetSuggestionVoteCommandHandler(IApplicationDbContext context)
    : IRequestHandler<SetSuggestionVoteCommand, Result<SetSuggestionVoteCommandResponse>>
{
    public async Task<Result<SetSuggestionVoteCommandResponse>> Handle(SetSuggestionVoteCommand request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions.FirstOrDefaultAsync(
            suggestion => suggestion.Id == request.SuggestionId,
            cancellationToken);

        if (suggestion is null)
        {
            throw new NotFoundException(nameof(Suggestion), request.SuggestionId);
        }

        if (!await context.Users.AsNoTracking().AnyAsync(user => user.Id == request.UserId && user.IsActive, cancellationToken))
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var existingVote = await context.Votes.FirstOrDefaultAsync(
            vote => vote.SuggestionId == request.SuggestionId && vote.UserId == request.UserId,
            cancellationToken);

        var upvoteCount = await context.Votes.CountAsync(
            vote => vote.SuggestionId == request.SuggestionId && vote.IsUpvote,
            cancellationToken);

        var downvoteCount = await context.Votes.CountAsync(
            vote => vote.SuggestionId == request.SuggestionId && !vote.IsUpvote,
            cancellationToken);

        if (existingVote is null)
        {
            var vote = new Vote
            {
                SuggestionId = request.SuggestionId,
                UserId = request.UserId,
                IsUpvote = request.IsUpvote
            };

            context.Votes.Add(vote);

            if (request.IsUpvote)
                upvoteCount++;
            else
                downvoteCount++;
        }
        else if (existingVote.IsUpvote != request.IsUpvote)
        {
            existingVote.IsUpvote = request.IsUpvote;

            if (request.IsUpvote)
            {
                upvoteCount++;
                downvoteCount--;
            }
            else
            {
                downvoteCount++;
                upvoteCount--;
            }
        }

        suggestion.VoteScore = upvoteCount - downvoteCount;

        await context.SaveChangesAsync(cancellationToken);

        return Result<SetSuggestionVoteCommandResponse>.Success(new SetSuggestionVoteCommandResponse(
            request.SuggestionId,
            upvoteCount,
            downvoteCount,
            suggestion.VoteScore,
            request.IsUpvote,
            request.IsUpvote ? "Öneriyi beğendiniz." : "Öneriyi beğenmediniz."));
    }
}
