using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllNotificationsAsReadCommand(Guid UserId) : IRequest<Result<MarkAllNotificationsAsReadCommandResponse>>;
