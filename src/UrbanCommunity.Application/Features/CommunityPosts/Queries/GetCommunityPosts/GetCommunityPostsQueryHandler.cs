using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.CommunityPosts;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts.Queries.GetCommunityPosts;

public sealed class GetCommunityPostsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCommunityPostsQuery, Result<IReadOnlyList<GetCommunityPostsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetCommunityPostsQueryResponse>>> Handle(GetCommunityPostsQuery request, CancellationToken cancellationToken)
    {
        var query = context.CommunityPosts
            .AsNoTracking()
            .Include(p => p.User)
            .AsQueryable();

        if (!request.IsAdmin)
        {
            var accessScope = await CommunityPostAccessPolicy.ResolveUserAccessScopeAsync(context, request.RequestingUserId, cancellationToken);
            query = query.WhereVisibleTo(accessScope);
        }

        if (request.Scope.HasValue)
            query = query.Where(p => p.Scope == request.Scope.Value);

        if (request.ProvinceId.HasValue)
            query = query.Where(p => p.Scope == CommunityPostScope.Province && p.ProvinceId == request.ProvinceId.Value);

        if (request.DistrictId.HasValue)
            query = query.Where(p => p.Scope == CommunityPostScope.District && p.DistrictId == request.DistrictId.Value);

        if (request.NeighborhoodId.HasValue)
            query = query.Where(p => p.Scope == CommunityPostScope.Neighborhood && p.NeighborhoodId == request.NeighborhoodId.Value);

        if (request.Status.HasValue)
            query = query.Where(p => p.Status == request.Status.Value);

        if (request.Type.HasValue)
            query = query.Where(p => p.Type == request.Type.Value);

        if (request.UserId.HasValue)
            query = query.Where(p => p.UserId == request.UserId.Value);

        var posts = await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new GetCommunityPostsQueryResponse(
                p.Id,
                p.Type,
                p.Title,
                p.Content,
                p.Status,
                p.UserId,
                $"{p.User.FirstName} {p.User.LastName}",
                p.Scope,
                p.ProvinceId,
                p.Province != null ? p.Province.Name : null,
                p.DistrictId,
                p.District != null ? p.District.Name : null,
                p.NeighborhoodId,
                p.Neighborhood != null ? p.Neighborhood.Name : null,
                p.Comments.Count,
                p.Media.Count,
                p.CreatedAt,
                p.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetCommunityPostsQueryResponse>>.Success(posts);
    }
}
