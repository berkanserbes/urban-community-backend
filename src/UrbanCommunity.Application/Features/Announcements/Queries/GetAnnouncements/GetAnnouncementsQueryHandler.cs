using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed class GetAnnouncementsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetAnnouncementsQuery, Result<IReadOnlyList<GetAnnouncementsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetAnnouncementsQueryResponse>>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Announcements
            .AsNoTracking()
            .Include(a => a.User)
            .Include(a => a.Neighborhood)
            .AsQueryable();

        if (request.NeighborhoodId.HasValue)
            query = query.Where(a => a.NeighborhoodId == request.NeighborhoodId.Value || a.NeighborhoodId == null);

        // IsActive belirtilmezse varsayılan olarak sadece aktif olanlar döner
        var activeFilter = request.IsActive ?? true;
        query = query.Where(a => a.IsActive == activeFilter);

        var now = DateTime.UtcNow;
        var announcements = await query
            .Where(a => !a.ExpiresAt.HasValue || a.ExpiresAt.Value > now)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new GetAnnouncementsQueryResponse(
                a.Id,
                a.Title,
                a.Content,
                a.UserId,
                $"{a.User.FirstName} {a.User.LastName}",
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
