using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed class GetSuggestionVotersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionVotersQuery, Result<PagedResult<VoterItem>>>
{
    public async Task<Result<PagedResult<VoterItem>>> Handle(GetSuggestionVotersQuery request, CancellationToken cancellationToken)
    {
        var suggestionExists = await context.Suggestions
            .AnyAsync(s => s.Id == request.SuggestionId, cancellationToken);

        if (!suggestionExists)
            throw new NotFoundException(nameof(Suggestion), request.SuggestionId);

        var query = context.Votes.AsNoTracking()
            .Where(v => v.SuggestionId == request.SuggestionId);

        if (request.IsUpvote.HasValue)
            query = query.Where(v => v.IsUpvote == request.IsUpvote.Value);

        var voters = await query
            .OrderBy(v => v.CreatedAt)
            .ThenBy(v => v.Id)
            .Select(v => new VoterItem(
                v.UserId,
                UserProjections.FullName.Invoke(v.User),
                v.CreatedAt,
                v.IsUpvote))
            .Expand()
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<VoterItem>>.Success(voters);
    }
}
