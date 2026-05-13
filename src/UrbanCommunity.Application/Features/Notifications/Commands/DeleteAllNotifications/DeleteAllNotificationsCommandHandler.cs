using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Notifications.Commands.DeleteAllNotifications;

public sealed class DeleteAllNotificationsCommandHandler(IApplicationDbContext context)
    : IRequestHandler<DeleteAllNotificationsCommand, Result<DeleteAllNotificationsCommandResponse>>
{
    public async Task<Result<DeleteAllNotificationsCommandResponse>> Handle(DeleteAllNotificationsCommand request, CancellationToken cancellationToken)
    {
        var notifications = await context.Notifications
            .Where(n => n.UserId == request.UserId)
            .ToListAsync(cancellationToken);

        context.Notifications.RemoveRange(notifications);
        await context.SaveChangesAsync(cancellationToken);

        return Result<DeleteAllNotificationsCommandResponse>.Success(
            new DeleteAllNotificationsCommandResponse(
                notifications.Count,
                $"{notifications.Count} bildirim başarıyla silindi."));
    }
}
