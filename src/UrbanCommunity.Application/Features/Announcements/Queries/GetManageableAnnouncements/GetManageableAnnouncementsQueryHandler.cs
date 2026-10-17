using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncements;

public sealed class GetManageableAnnouncementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetManageableAnnouncementsQuery, Result<PagedResult<GetAnnouncementsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetAnnouncementsQueryResponse>>> Handle(
        GetManageableAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Announcements
            .AsNoTracking()
            .AsQueryable();

        if (!request.IsAdmin)
        {
            query = query.Where(a => a.UserId == request.RequestingUserId);
        }

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
}
