using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Announcements.Commands.CreateAnnouncement;

public sealed record CreateAnnouncementCommandResponse(
    Guid Id,
    string Title,
    string Content,
    AnnouncementScope Scope,
    bool IsActive,
    DateTime? ExpiresAt,
    DateTime CreatedAt);