namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsUnread;

public sealed record MarkNotificationAsUnreadCommandResponse(Guid Id, string Message);
