namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommandResponse(
    Guid Id,
    string Title,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime CreatedAt);
