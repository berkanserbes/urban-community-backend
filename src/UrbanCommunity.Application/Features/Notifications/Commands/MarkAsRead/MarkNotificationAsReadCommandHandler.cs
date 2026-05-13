using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsRead;

public sealed class MarkNotificationAsReadCommandHandler(IApplicationDbContext context)
    : IRequestHandler<MarkNotificationAsReadCommand, Result<MarkNotificationAsReadCommandResponse>>
{
    public async Task<Result<MarkNotificationAsReadCommandResponse>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await context.Notifications
            .FirstOrDefaultAsync(n => n.Id == request.NotificationId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.NotificationId);

        if (notification.UserId != request.UserId)
            throw new ForbiddenAccessException("Bu bildirime erişim yetkiniz yok.");

        if (notification.IsRead)
        {
            return Result<MarkNotificationAsReadCommandResponse>.Failure(["Bu bildirim zaten okundu."]);
        }

        notification.MarkAsRead();
        await context.SaveChangesAsync(cancellationToken);

        return Result<MarkNotificationAsReadCommandResponse>.Success(
            new MarkNotificationAsReadCommandResponse(notification.Id, "Bildirim okundu olarak işaretlendi."));
    }
}
