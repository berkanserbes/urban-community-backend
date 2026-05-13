using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsRead;

public sealed record MarkNotificationAsReadCommand(Guid NotificationId, Guid UserId) : IRequest<Result<MarkNotificationAsReadCommandResponse>>;
