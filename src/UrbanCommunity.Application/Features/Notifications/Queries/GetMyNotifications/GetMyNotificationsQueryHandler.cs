using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed class GetMyNotificationsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetMyNotificationsQuery, Result<IReadOnlyList<GetMyNotificationsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetMyNotificationsQueryResponse>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == request.UserId);

        if (request.IsRead.HasValue)
            query = query.Where(n => n.IsRead == request.IsRead.Value);

        var notifications = await query
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Select(n => new GetMyNotificationsQueryResponse(
                n.Id,
                n.Title,
                n.Message,
                n.IsRead,
                n.RelatedEntityId,
                n.RelatedEntityType.HasValue ? n.RelatedEntityType.Value.ToString() : null,
                n.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetMyNotificationsQueryResponse>>.Success(notifications);
    }
}
