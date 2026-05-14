using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed class GetSuggestionVotersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionVotersQuery, Result<GetSuggestionVotersQueryResponse>>
{
    public async Task<Result<GetSuggestionVotersQueryResponse>> Handle(GetSuggestionVotersQuery request, CancellationToken cancellationToken)
    {
        var suggestionExists = await context.Suggestions
            .AnyAsync(s => s.Id == request.SuggestionId, cancellationToken);

        if (!suggestionExists)
            throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        var votes = await context.Votes
            .Where(v => v.SuggestionId == request.SuggestionId)
            .Include(v => v.User)
            .OrderBy(v => v.CreatedAt)
            .ToListAsync(cancellationToken);

        var upvoters = votes
            .Where(v => v.IsUpvote)
            .Select(v => new VoterItem(v.UserId, v.User.FullName, v.CreatedAt))
            .ToList();

        var downvoters = votes
            .Where(v => !v.IsUpvote)
            .Select(v => new VoterItem(v.UserId, v.User.FullName, v.CreatedAt))
            .ToList();

        return Result<GetSuggestionVotersQueryResponse>.Success(
            new GetSuggestionVotersQueryResponse(
                upvoters.Count,
                downvoters.Count,
                upvoters,
                downvoters));
    }
}
