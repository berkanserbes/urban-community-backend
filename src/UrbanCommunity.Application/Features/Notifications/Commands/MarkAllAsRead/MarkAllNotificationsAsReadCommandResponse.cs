namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllNotificationsAsReadCommandResponse(int MarkedCount, string Message);
