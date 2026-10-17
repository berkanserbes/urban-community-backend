using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;

public sealed class GetSuggestionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetSuggestionsQuery, Result<PagedResult<GetSuggestionsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetSuggestionsQueryResponse>>> Handle(GetSuggestionsQuery request, CancellationToken cancellationToken)
    {
        var requestingUserId = request.RequestingUserId;

        var query = context.Suggestions
            .AsNoTracking()
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        if (request.UserId.HasValue)
            query = query.Where(s => s.UserId == request.UserId.Value);

        if (request.NeighborhoodId.HasValue)
            query = query.Where(s => s.User.Neighborhood != null && s.User.Neighborhood.Id == request.NeighborhoodId.Value);

        if (request.DistrictId.HasValue)
            query = query.Where(s => s.User.Neighborhood != null && s.User.Neighborhood.DistrictId == request.DistrictId.Value);

        if (request.ProvinceId.HasValue)
            query = query.Where(s => s.User.Neighborhood != null && s.User.Neighborhood.DistrictNavigation.ProvinceId == request.ProvinceId.Value);

        var suggestions = await query
            .OrderByDescending(s => s.VoteScore)
            .ThenByDescending(s => s.CreatedAt)
            .ThenBy(s => s.Id)
            .Select(s => new GetSuggestionsQueryResponse(
                s.Id,
                s.Title,
                s.Content,
                s.OfficialResponse,
                s.Status,
                s.VoteScore,
                s.Votes.Count(v => v.IsUpvote),
                s.Votes.Count(v => !v.IsUpvote),
                s.UserId,
                UserProjections.FullName.Invoke(s.User),
                requestingUserId.HasValue
                    ? s.Votes
                        .Where(v => v.UserId == requestingUserId.Value)
                        .Select(v => (bool?)v.IsUpvote)
                        .FirstOrDefault()
                    : null,
                s.CreatedAt))
            .Expand()
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<GetSuggestionsQueryResponse>>.Success(suggestions);
    }
}
