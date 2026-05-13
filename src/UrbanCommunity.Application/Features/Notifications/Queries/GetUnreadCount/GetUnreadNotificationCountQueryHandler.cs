using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed class GetUnreadNotificationCountQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUnreadNotificationCountQuery, Result<GetUnreadNotificationCountQueryResponse>>
{
    public async Task<Result<GetUnreadNotificationCountQueryResponse>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
    {
        var count = await context.Notifications
            .AsNoTracking()
            .CountAsync(n => n.UserId == request.UserId && !n.IsRead, cancellationToken);

        return Result<GetUnreadNotificationCountQueryResponse>.Success(
            new GetUnreadNotificationCountQueryResponse(count));
    }
}
