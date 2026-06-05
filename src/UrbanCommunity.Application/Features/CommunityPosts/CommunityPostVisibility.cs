using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.CommunityPosts;

internal sealed record CommunityPostViewerLocation(
    Guid? NeighborhoodId,
    Guid? DistrictId,
    Guid? ProvinceId);

internal static class CommunityPostVisibility
{
    internal static async Task<CommunityPostViewerLocation> GetViewerLocationAsync(
        IApplicationDbContext context,
        Guid userId,
        CancellationToken cancellationToken)
    {
        var location = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => new CommunityPostViewerLocation(
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
        CommunityPostViewerLocation location)
    {
        return query.Where(post =>
            (post.Scope == CommunityPostScope.Neighborhood && post.NeighborhoodId == location.NeighborhoodId) ||
            (post.Scope == CommunityPostScope.District && post.DistrictId == location.DistrictId) ||
            (post.Scope == CommunityPostScope.Province && post.ProvinceId == location.ProvinceId));
    }

    internal static bool IsVisibleTo(this CommunityPost post, CommunityPostViewerLocation location)
    {
        return post.Scope switch
        {
            CommunityPostScope.Neighborhood => post.NeighborhoodId.HasValue
                && location.NeighborhoodId.HasValue
                && post.NeighborhoodId == location.NeighborhoodId,
            CommunityPostScope.District => post.DistrictId.HasValue
                && location.DistrictId.HasValue
                && post.DistrictId == location.DistrictId,
            CommunityPostScope.Province => post.ProvinceId.HasValue
                && location.ProvinceId.HasValue
                && post.ProvinceId == location.ProvinceId,
            _ => false
        };
    }

    internal static bool CanCreateInScope(
        CommunityPostScope scope,
        Guid? provinceId,
        Guid? districtId,
        Guid? neighborhoodId,
        CommunityPostViewerLocation location)
    {
        return scope switch
        {
            CommunityPostScope.Neighborhood => neighborhoodId.HasValue
                && location.NeighborhoodId.HasValue
                && neighborhoodId == location.NeighborhoodId,
            CommunityPostScope.District => districtId.HasValue
                && location.DistrictId.HasValue
                && districtId == location.DistrictId,
            CommunityPostScope.Province => provinceId.HasValue
                && location.ProvinceId.HasValue
                && provinceId == location.ProvinceId,
            _ => false
        };
    }
}
