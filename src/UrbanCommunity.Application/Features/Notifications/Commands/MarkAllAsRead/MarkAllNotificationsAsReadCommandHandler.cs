using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed class MarkAllNotificationsAsReadCommandHandler(IApplicationDbContext context)
    : IRequestHandler<MarkAllNotificationsAsReadCommand, Result<MarkAllNotificationsAsReadCommandResponse>>
{
    public async Task<Result<MarkAllNotificationsAsReadCommandResponse>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
    {
        var unreadNotifications = await context.Notifications
            .Where(n => n.UserId == request.UserId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var notification in unreadNotifications)
        {
            notification.MarkAsRead();
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result<MarkAllNotificationsAsReadCommandResponse>.Success(
            new MarkAllNotificationsAsReadCommandResponse(
                unreadNotifications.Count,
                $"{unreadNotifications.Count} bildirim okundu olarak işaretlendi."));
    }
}
