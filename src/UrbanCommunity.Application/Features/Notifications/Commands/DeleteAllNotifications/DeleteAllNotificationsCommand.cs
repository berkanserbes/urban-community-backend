using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Commands.DeleteAllNotifications;

public sealed record DeleteAllNotificationsCommand(Guid UserId)
    : IRequest<Result<DeleteAllNotificationsCommandResponse>>;
