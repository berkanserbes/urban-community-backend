using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoteSummary;

public sealed class GetSuggestionVoteSummaryQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionVoteSummaryQuery, Result<GetSuggestionVoteSummaryQueryResponse>>
{
    public async Task<Result<GetSuggestionVoteSummaryQueryResponse>> Handle(GetSuggestionVoteSummaryQuery request, CancellationToken cancellationToken)
    {
        var suggestion = await context.Suggestions
            .AsNoTracking()
            .FirstOrDefaultAsync(suggestion => suggestion.Id == request.SuggestionId, cancellationToken);

        if (suggestion is null)
        {
            throw new NotFoundException(nameof(Suggestion), request.SuggestionId);
        }

        var upvoteCount = await context.Votes.CountAsync(
            vote => vote.SuggestionId == request.SuggestionId && vote.IsUpvote,
            cancellationToken);

        var downvoteCount = await context.Votes.CountAsync(
            vote => vote.SuggestionId == request.SuggestionId && !vote.IsUpvote,
            cancellationToken);

        bool? currentUserIsUpvote = null;

        if (request.UserId.HasValue)
        {
            currentUserIsUpvote = await context.Votes
                .AsNoTracking()
                .Where(vote => vote.SuggestionId == request.SuggestionId && vote.UserId == request.UserId.Value)
                .Select(vote => (bool?)vote.IsUpvote)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return Result<GetSuggestionVoteSummaryQueryResponse>.Success(new GetSuggestionVoteSummaryQueryResponse(
            request.SuggestionId,
            upvoteCount,
            downvoteCount,
            suggestion.VoteScore,
            currentUserIsUpvote));
    }
}
