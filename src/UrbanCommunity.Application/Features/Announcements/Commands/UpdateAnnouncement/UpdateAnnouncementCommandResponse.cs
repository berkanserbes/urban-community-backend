namespace UrbanCommunity.Application.Features.Announcements.Commands.UpdateAnnouncement;

public sealed record UpdateAnnouncementCommandResponse(
    Guid Id,
    string Title,
    string Content,
    DateTime? ExpiresAt,
    DateTime UpdatedAt);
