namespace UrbanCommunity.Application.Features.Notifications.Commands.MarkAsRead;

public sealed record MarkNotificationAsReadCommandResponse(Guid Id, string Message);
