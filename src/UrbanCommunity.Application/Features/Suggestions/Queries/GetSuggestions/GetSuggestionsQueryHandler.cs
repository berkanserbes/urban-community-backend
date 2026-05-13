using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;

public sealed class GetSuggestionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionsQuery, Result<IReadOnlyList<GetSuggestionsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetSuggestionsQueryResponse>>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Suggestions
            .AsNoTracking()
            .Include(s => s.User)
            .Include(s => s.Votes)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.UserId.HasValue)
            query = query.Where(s => s.UserId == request.UserId.Value);

        var suggestions = await query
            .OrderByDescending(s => s.VoteScore)
            .ThenByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var result = suggestions
            .Select(s => new GetSuggestionsQueryResponse(
                s.Id,
                s.Title,
                s.Content,
                s.Status,
                s.VoteScore,
                s.Votes.Count(v => v.IsUpvote),
                s.Votes.Count(v => !v.IsUpvote),
                s.UserId,
                s.User.FullName,
                s.CreatedAt))
            .ToList();

        return Result<IReadOnlyList<GetSuggestionsQueryResponse>>.Success(result);
    }
}
