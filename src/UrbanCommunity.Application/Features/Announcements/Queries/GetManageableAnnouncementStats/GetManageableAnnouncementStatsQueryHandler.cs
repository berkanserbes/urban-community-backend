using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Queries.GetManageableAnnouncementStats;

public sealed class GetManageableAnnouncementStatsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetManageableAnnouncementStatsQuery, Result<GetManageableAnnouncementStatsQueryResponse>>
{
    /// <summary>Announcements expiring within this window (or already expired) count as expiring soon.</summary>
    private static readonly TimeSpan ExpiringSoonWindow = TimeSpan.FromDays(5);

    public async Task<Result<GetManageableAnnouncementStatsQueryResponse>> Handle(
        GetManageableAnnouncementStatsQuery request,
        CancellationToken cancellationToken)
    {
        var expiringBefore = DateTime.UtcNow.Add(ExpiringSoonWindow);
        var query = context.Announcements.AsNoTracking();

        if (!request.IsAdmin)
            query = query.Where(a => a.UserId == request.RequestingUserId);

        var stats = await query
            .GroupBy(_ => 1)
            .Select(announcements => new GetManageableAnnouncementStatsQueryResponse(
                announcements.Count(),
                announcements.Count(a => a.IsActive),
                announcements.Count(a => a.Scope == AnnouncementScope.All),
                announcements.Count(a => a.ExpiresAt.HasValue && a.ExpiresAt.Value <= expiringBefore)))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetManageableAnnouncementStatsQueryResponse>.Success(
            stats ?? GetManageableAnnouncementStatsQueryResponse.Empty);
    }
}
