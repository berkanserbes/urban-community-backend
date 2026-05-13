using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsUnread;

public sealed record MarkNotificationAsUnreadCommand(Guid NotificationId, Guid UserId)
    : IRequest<Result<MarkNotificationAsUnreadCommandResponse>>;
