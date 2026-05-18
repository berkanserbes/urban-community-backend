namespace UrbanCommunity.Application.Features.Announcements.Commands.DeactivateAnnouncement;

public sealed record DeactivateAnnouncementCommandResponse(
    Guid Id,
    string Message);
