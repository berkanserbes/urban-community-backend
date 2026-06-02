using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed class GetAnnouncementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAnnouncementsQuery, Result<IReadOnlyList<GetAnnouncementsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetAnnouncementsQueryResponse>>> Handle(
        GetAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query = context.Announcements
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Province)
            .Include(a => a.District)
            .Include(a => a.Neighborhood)
            .AsQueryable();

        query = query.Where(a => !a.ExpiresAt.HasValue || a.ExpiresAt.Value > now);
        query = query.Where(a => a.IsActive == (request.IsActive ?? true));

        // Location-based scope filter:
        //  NeighborhoodId → All + Province + District + Neighborhood
        //  DistrictId     → All + Province + District
        //  ProvinceId     → All + Province
        //  (hiçbiri)      → All
        query = BuildLocationFilter(query, request);

        var announcements = await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new GetAnnouncementsQueryResponse(
                a.Id,
                a.Title,
                a.Content,
                a.UserId,
                a.User.GetUserFullName(),
                a.Scope,
                a.ProvinceId,
                a.Province != null ? a.Province.Name : null,
                a.DistrictId,
                a.District != null ? a.District.Name : null,
                a.NeighborhoodId,
                a.Neighborhood != null ? a.Neighborhood.Name : null,
                a.IsActive,
                a.ExpiresAt,
                a.CreatedAt,
                a.UpdatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetAnnouncementsQueryResponse>>.Success(announcements);
    }

    private static IQueryable<Domain.Entities.Announcement> BuildLocationFilter(
        IQueryable<Domain.Entities.Announcement> query,
        GetAnnouncementsQuery request)
    {
        if (request.NeighborhoodId.HasValue)
        {
            return query.Where(a =>
                a.Scope == AnnouncementScope.All ||
                (a.Scope == AnnouncementScope.Province     && a.ProvinceId     == request.ProvinceId) ||
                (a.Scope == AnnouncementScope.District     && a.DistrictId     == request.DistrictId) ||
                (a.Scope == AnnouncementScope.Neighborhood && a.NeighborhoodId == request.NeighborhoodId));
        }

        if (request.DistrictId.HasValue)
        {
            return query.Where(a =>
                a.Scope == AnnouncementScope.All ||
                (a.Scope == AnnouncementScope.Province && a.ProvinceId == request.ProvinceId) ||
                (a.Scope == AnnouncementScope.District && a.DistrictId == request.DistrictId));
        }

        if (request.ProvinceId.HasValue)
        {
            return query.Where(a =>
                a.Scope == AnnouncementScope.All ||
                (a.Scope == AnnouncementScope.Province && a.ProvinceId == request.ProvinceId));
        }

        return query.Where(a => a.Scope == AnnouncementScope.All);
    }
}
