using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Commands.DeleteNotification;

public sealed record DeleteNotificationCommand(Guid NotificationId, Guid UserId)
    : IRequest<Result<DeleteNotificationCommandResponse>>;
