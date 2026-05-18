namespace UrbanCommunity.Application.Features.Announcements.Queries.GetAnnouncements;

public sealed record GetAnnouncementsQueryResponse(
    Guid Id,
    string Title,
    string Content,
    Guid UserId,
    string UserFullName,
    Guid? NeighborhoodId,
    string? NeighborhoodName,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
