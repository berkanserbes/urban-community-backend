using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsUnread;

public sealed class MarkNotificationAsUnreadCommandHandler(IApplicationDbContext context)
    : IRequestHandler<MarkNotificationAsUnreadCommand, Result<MarkNotificationAsUnreadCommandResponse>>
{
    public async Task<Result<MarkNotificationAsUnreadCommandResponse>> Handle(MarkNotificationAsUnreadCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (notification.UserId != request.UserId)
            throw new ForbiddenAccessException("Bu bildirime erişim yetkiniz yok.");

        if (!notification.IsRead)
        {
            return Result<MarkNotificationAsUnreadCommandResponse>.Failure(["Bu bildirim zaten okunmadı."]);
        }

        notification.MarkAsUnread();
        await context.SaveChangesAsync(cancellationToken);

        return Result<MarkNotificationAsUnreadCommandResponse>.Success(
            new MarkNotificationAsUnreadCommandResponse(notification.Id, "Bildirim okunmadı olarak işaretlendi."));
    }
}
