using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts;

internal sealed record CommunityPostAccessScope(
    Guid? NeighborhoodId,
    Guid? DistrictId,
    Guid? ProvinceId);

internal static class CommunityPostAccessPolicy
{
    internal static async Task<CommunityPostAccessScope> ResolveUserAccessScopeAsync(
        IApplicationDbContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var location = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => new CommunityPostAccessScope(
                user.NeighborhoodId,
                user.Neighborhood != null ? user.Neighborhood.DistrictId : null,
                user.Neighborhood != null && user.Neighborhood.DistrictNavigation != null
                    ? user.Neighborhood.DistrictNavigation.ProvinceId
                    : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
        {
            throw new NotFoundException(nameof(User), userId);
        }

        if (!location.NeighborhoodId.HasValue)
        {
            throw new ForbiddenAccessException("Topluluk gönderileri için kullanıcı mahalle bilgisi gereklidir.");
        }

        return location;
    }

    internal static IQueryable<CommunityPost> WhereVisibleTo(
        this IQueryable<CommunityPost> query,
        CommunityPostAccessScope accessScope)
    {
        return query.Where(post =>
            (post.Scope == CommunityPostScope.Neighborhood && post.NeighborhoodId == accessScope.NeighborhoodId) ||
            (post.Scope == CommunityPostScope.District && post.DistrictId == accessScope.DistrictId) ||
            (post.Scope == CommunityPostScope.Province && post.ProvinceId == accessScope.ProvinceId));
    }

    internal static bool IsVisibleTo(this CommunityPost post, CommunityPostAccessScope accessScope)
    {
        return post.Scope switch
        {
            CommunityPostScope.Neighborhood => post.NeighborhoodId.HasValue
                && accessScope.NeighborhoodId.HasValue
                && post.NeighborhoodId == accessScope.NeighborhoodId,
            CommunityPostScope.District => post.DistrictId.HasValue
                && accessScope.DistrictId.HasValue
                && post.DistrictId == accessScope.DistrictId,
            CommunityPostScope.Province => post.ProvinceId.HasValue
                && accessScope.ProvinceId.HasValue
                && post.ProvinceId == accessScope.ProvinceId,
            _ => false
        };
    }

    internal static bool CanCreateInScope(
        CommunityPostScope scope,
        Guid? provinceId,
        Guid? districtId,
        Guid? neighborhoodId,
        CommunityPostAccessScope accessScope)
    {
        return scope switch
        {
            CommunityPostScope.Neighborhood => neighborhoodId.HasValue
                && accessScope.NeighborhoodId.HasValue
                && neighborhoodId == accessScope.NeighborhoodId,
            CommunityPostScope.District => districtId.HasValue
                && accessScope.DistrictId.HasValue
                && districtId == accessScope.DistrictId,
            CommunityPostScope.Province => provinceId.HasValue
                && accessScope.ProvinceId.HasValue
                && provinceId == accessScope.ProvinceId,
            _ => false
        };
    }
}
