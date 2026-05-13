using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Notifications.Commands.DeleteNotification;

public sealed class DeleteNotificationCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteNotificationCommand, Result<DeleteNotificationCommandResponse>>
{
    public async Task<Result<DeleteNotificationCommandResponse>> Handle(DeleteNotificationCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (notification.UserId != request.UserId)
            throw new ForbiddenAccessException("Bu bildirime erişim yetkiniz yok.");

        context.Notifications.Remove(notification);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteNotificationCommandResponse>.Success(
            new DeleteNotificationCommandResponse("Bildirim başarıyla silindi."));
    }
}
