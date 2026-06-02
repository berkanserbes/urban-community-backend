using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Events;

public record AnnouncementPublishedEvent
{
    public Guid AnnouncementId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public Guid PublishedByUserId { get; init; }
    public string PublishedByUserFullName { get; init; } = string.Empty;

    public AnnouncementScope Scope { get; init; } = AnnouncementScope.All;
    public Guid? ProvinceId { get; init; }
    public Guid? DistrictId { get; init; }
    public Guid? NeighborhoodId { get; init; }

    public DateTime? ExpiresAt { get; init; }
    public DateTime CreatedAt { get; init; }
}
