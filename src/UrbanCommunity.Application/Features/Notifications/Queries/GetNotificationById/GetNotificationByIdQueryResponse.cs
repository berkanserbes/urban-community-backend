namespace UrbanCommunity.Application.Features.Notifications.Queries.GetNotificationById;

public sealed record GetNotificationByIdQueryResponse(
    Guid Id,
    string Title,
    string Message,
    bool IsRead,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    DateTime CreatedAt);
