namespace UrbanCommunity.Application.Features.Notifications.Queries.GetMyNotifications;

public sealed record GetMyNotificationsQueryResponse(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    DateTime CreatedAt);
