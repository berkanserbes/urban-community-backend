using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Application.Realtime;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

/// <summary>
/// Creates persistent notifications and sends the announcement to the
/// server-managed SignalR group matching its location scope.
/// </summary>
public sealed class AnnouncementPublishedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService) : IConsumer<AnnouncementPublishedEvent>
{
    public async Task Consume(ConsumeContext<AnnouncementPublishedEvent> consumeContext)
    {
        var evt = consumeContext.Message;
        var cancellationToken = consumeContext.CancellationToken;

        var recipientIds = await BuildRecipientQuery(evt)
            .Where(user => user.Id != evt.PublishedByUserId)
            .Select(user => user.Id)
            .ToListAsync(cancellationToken);

        if (recipientIds.Count > 0)
        {
            var existingRecipientIds = await context.Notifications
                .AsNoTracking()
                .Where(notification =>
                    notification.RelatedEntityId == evt.AnnouncementId &&
                    notification.RelatedEntityType == NotificationRelatedEntityType.Announcement &&
                    recipientIds.Contains(notification.UserId))
                .Select(notification => notification.UserId)
                .ToListAsync(cancellationToken);

            var existingRecipientIdSet = existingRecipientIds.ToHashSet();
            var notifications = recipientIds
                .Where(userId => !existingRecipientIdSet.Contains(userId))
                .Select(userId => new Notification
                {
                    UserId = userId,
                    Title = "Yeni Duyuru",
                    Message = $"\"{evt.Title}\" başlıklı yeni bir duyuru yayınlandı.",
                    IsRead = false,
                    RelatedEntityId = evt.AnnouncementId,
                    RelatedEntityType = NotificationRelatedEntityType.Announcement
                })
                .ToList();

            if (notifications.Count > 0)
            {
                context.Notifications.AddRange(notifications);
                await context.SaveChangesAsync(cancellationToken);

                await Task.WhenAll(notifications.Select(n =>
                    notificationService.SendToUserAsync(
                        n.UserId.ToString(),
                        "NewNotification",
                        new
                        {
                            id = n.Id,
                            title = n.Title,
                            message = n.Message,
                            isRead = n.IsRead,
                            relatedEntityId = n.RelatedEntityId,
                            relatedEntityType = n.RelatedEntityType?.ToString(),
                            createdAt = n.CreatedAt
                        },
                        cancellationToken)));
            }
        }

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
                => NotificationGroupNames.Neighborhood(evt.NeighborhoodId.Value),

            AnnouncementScope.District when evt.DistrictId.HasValue
                => NotificationGroupNames.District(evt.DistrictId.Value),

            AnnouncementScope.Province when evt.ProvinceId.HasValue
                => NotificationGroupNames.Province(evt.ProvinceId.Value),

            _ => NotificationGroupNames.All
        };

        await notificationService.SendToGroupAsync(
            groupName,
            "NewAnnouncement",
            payload,
            cancellationToken);
    }

    private IQueryable<User> BuildRecipientQuery(AnnouncementPublishedEvent evt)
    {
        var users = context.Users
            .AsNoTracking()
            .Where(user => user.IsActive);

        return evt.Scope switch
        {
            AnnouncementScope.All => users,

            AnnouncementScope.Province when evt.ProvinceId.HasValue =>
                users.Where(user =>
                    user.Neighborhood != null &&
                    user.Neighborhood.DistrictNavigation != null &&
                    user.Neighborhood.DistrictNavigation.ProvinceId == evt.ProvinceId.Value),

            AnnouncementScope.District when evt.DistrictId.HasValue =>
                users.Where(user =>
                    user.Neighborhood != null &&
                    user.Neighborhood.DistrictId == evt.DistrictId.Value),

            AnnouncementScope.Neighborhood when evt.NeighborhoodId.HasValue =>
                users.Where(user => user.NeighborhoodId == evt.NeighborhoodId.Value),

            _ => users.Where(_ => false)
        };
    }
}
