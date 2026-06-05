using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Notifications.Queries.GetNotificationById;

public sealed class GetNotificationByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNotificationByIdQuery, Result<GetNotificationByIdQueryResponse>>
{
    public async Task<Result<GetNotificationByIdQueryResponse>> Handle(
        GetNotificationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .AsNoTracking()
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (!request.IsAdmin && notification.UserId != request.UserId)
            throw new ForbiddenAccessException("Bu bildirime erişim yetkiniz yok.");

        return Result<GetNotificationByIdQueryResponse>.Success(new GetNotificationByIdQueryResponse(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.IsRead,
            notification.RelatedEntityId,
            notification.RelatedEntityType.HasValue ? notification.RelatedEntityType.Value.ToString() : null,
            notification.CreatedAt));
    }
}
