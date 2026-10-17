using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed class GetAnnouncementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAnnouncementsQuery, Result<PagedResult<GetAnnouncementsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetAnnouncementsQueryResponse>>> Handle(
        GetAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var query = context.Announcements
            .AsNoTracking()
            .AsQueryable();

        query = query.Where(a => !a.ExpiresAt.HasValue || a.ExpiresAt.Value > now);
        query = query.Where(a => a.IsActive == (request.IsActive ?? true));

        var location = await ResolveLocationAsync(request, cancellationToken);
        query = BuildLocationFilter(query, location);

        var announcements = await query
            .OrderByDescending(a => a.CreatedAt)
            .ThenBy(a => a.Id)
            .Select(a => new GetAnnouncementsQueryResponse(
                a.Id,
                a.Title,
                a.Content,
                a.UserId,
                UserProjections.FullName.Invoke(a.User),
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
            .Expand()
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<GetAnnouncementsQueryResponse>>.Success(announcements);
    }

    private static IQueryable<Announcement> BuildLocationFilter(
        IQueryable<Announcement> query,
        AnnouncementLocationFilter location)
    {
        if (location.NeighborhoodId.HasValue)
        {
            return query.Where(a =>
                a.Scope == AnnouncementScope.All ||
                (a.Scope == AnnouncementScope.Province && a.ProvinceId == location.ProvinceId) ||
                (a.Scope == AnnouncementScope.District && a.DistrictId == location.DistrictId) ||
                (a.Scope == AnnouncementScope.Neighborhood && a.NeighborhoodId == location.NeighborhoodId));
        }

        if (location.DistrictId.HasValue)
        {
            return query.Where(a =>
                a.Scope == AnnouncementScope.All ||
                (a.Scope == AnnouncementScope.Province && a.ProvinceId == location.ProvinceId) ||
                (a.Scope == AnnouncementScope.District && a.DistrictId == location.DistrictId));
        }

        if (location.ProvinceId.HasValue)
        {
            return query.Where(a =>
                a.Scope == AnnouncementScope.All ||
                (a.Scope == AnnouncementScope.Province && a.ProvinceId == location.ProvinceId));
        }

        return query.Where(a => a.Scope == AnnouncementScope.All);
    }

    private async Task<AnnouncementLocationFilter> ResolveLocationAsync(
        GetAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.NeighborhoodId.HasValue)
        {
            var location = await context.Neighborhoods
                .AsNoTracking()
                .Where(n => n.Id == request.NeighborhoodId.Value)
                .Select(n => new
                {
                    NeighborhoodId = n.Id,
                    DistrictId = n.DistrictId,
                    ProvinceId = n.DistrictNavigation != null ? n.DistrictNavigation.ProvinceId : (Guid?)null
                })
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId.Value);

            return new AnnouncementLocationFilter(location.ProvinceId, location.DistrictId, location.NeighborhoodId);
        }

        if (request.DistrictId.HasValue)
        {
            var location = await context.Districts
                .AsNoTracking()
                .Where(d => d.Id == request.DistrictId.Value)
                .Select(d => new AnnouncementLocationFilter(d.ProvinceId, d.Id, null))
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new NotFoundException(nameof(District), request.DistrictId.Value);

            return location;
        }

        if (request.ProvinceId.HasValue)
        {
            var exists = await context.Provinces
                .AsNoTracking()
                .AnyAsync(p => p.Id == request.ProvinceId.Value, cancellationToken);

            if (!exists)
                throw new NotFoundException(nameof(Province), request.ProvinceId.Value);
        }

        return new AnnouncementLocationFilter(request.ProvinceId, null, null);
    }

    private sealed record AnnouncementLocationFilter(Guid? ProvinceId, Guid? DistrictId, Guid? NeighborhoodId);
}
