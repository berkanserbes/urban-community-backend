using MassTransit;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

/// <summary>
/// Sends real-time push via SignalR when an announcement is published.
/// Uses group-based messaging — clients join relevant groups on connect
/// ("all", "province_{id}", "district_{id}", "neighborhood_{id}").
/// </summary>
public sealed class AnnouncementPublishedConsumer(
    IRealtimeNotificationService notificationService) : IConsumer<AnnouncementPublishedEvent>
{
    public async Task Consume(ConsumeContext<AnnouncementPublishedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        var payload = new
        {
            announcementId = evt.AnnouncementId,
            title = evt.Title,
            content = evt.Content,
            publishedBy = evt.PublishedByUserFullName,
            scope = evt.Scope.ToString(),
            expiresAt = evt.ExpiresAt,
            createdAt = evt.CreatedAt
        };

        var groupName = evt.Scope switch
        {
            AnnouncementScope.Neighborhood when evt.NeighborhoodId.HasValue
                => $"neighborhood_{evt.NeighborhoodId.Value}",

            AnnouncementScope.District when evt.DistrictId.HasValue
                => $"district_{evt.DistrictId.Value}",

            AnnouncementScope.Province when evt.ProvinceId.HasValue
                => $"province_{evt.ProvinceId.Value}",

            _ => "all"
        };

        await notificationService.SendToGroupAsync(
            groupName,
            "NewAnnouncement",
            payload,
            consumeContext.CancellationToken);
    }
}