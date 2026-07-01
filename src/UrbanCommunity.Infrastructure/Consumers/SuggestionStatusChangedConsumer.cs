using MassTransit;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class SuggestionStatusChangedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService) : IConsumer<SuggestionStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<SuggestionStatusChangedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        var statusLabel = evt.NewStatus switch
        {
            SuggestionStatus.Active => "Aktif",
            SuggestionStatus.Reviewing => "İncelemede",
            SuggestionStatus.Implementation => "Uygulamada",
            SuggestionStatus.Closed => "Kapatıldı",
            _ => evt.NewStatus.ToString()
        };

        var notification = new Notification
        {
            UserId = evt.SuggestionOwnerId,
            Title = "Öneri Durumu Güncellendi",
            Message = $"\"{evt.SuggestionTitle}\" başlıklı önerinizin durumu \"{statusLabel}\" olarak güncellendi.",
            IsRead = false,
            RelatedEntityId = evt.SuggestionId,
            RelatedEntityType = NotificationRelatedEntityType.Suggestion
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(consumeContext.CancellationToken);

        var newNotificationPayload = new
        {
            id = notification.Id,
            title = notification.Title,
            message = notification.Message,
            isRead = notification.IsRead,
            relatedEntityId = notification.RelatedEntityId,
            relatedEntityType = notification.RelatedEntityType?.ToString(),
            createdAt = notification.CreatedAt
        };

        await Task.WhenAll(
            notificationService.SendToUserAsync(
                evt.SuggestionOwnerId.ToString(),
                "SuggestionStatusChanged",
                new
                {
                    suggestionId = evt.SuggestionId,
                    suggestionTitle = evt.SuggestionTitle,
                    oldStatus = evt.OldStatus.ToString(),
                    newStatus = evt.NewStatus.ToString(),
                    changedAt = evt.ChangedAt
                },
                consumeContext.CancellationToken),
            notificationService.SendToUserAsync(
                evt.SuggestionOwnerId.ToString(),
                "NewNotification",
                newNotificationPayload,
                consumeContext.CancellationToken)
        );
    }
}
