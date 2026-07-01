using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncements;

public sealed class GetManageableAnnouncementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetManageableAnnouncementsQuery, Result<IReadOnlyList<GetAnnouncementsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetAnnouncementsQueryResponse>>> Handle(
        GetManageableAnnouncementsQuery request,
        CancellationToken cancellationToken)
    {
        var query = context.Announcements
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Province)
            .Include(a => a.District)
            .Include(a => a.Neighborhood)
            .AsQueryable();

        if (!request.IsAdmin)
        {
            query = query.Where(a => a.UserId == request.RequestingUserId);
        }

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
}
