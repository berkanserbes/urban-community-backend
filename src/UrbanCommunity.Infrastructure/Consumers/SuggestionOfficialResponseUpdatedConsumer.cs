using MassTransit;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class SuggestionOfficialResponseUpdatedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService) : IConsumer<SuggestionOfficialResponseUpdatedEvent>
{
    public async Task Consume(ConsumeContext<SuggestionOfficialResponseUpdatedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        var notification = new Notification
        {
            UserId = evt.SuggestionOwnerId,
            Title = "Önerinize Resmi Yanıt Verildi",
            Message = $"\"{evt.SuggestionTitle}\" başlıklı önerinize resmi bir yanıt verildi.",
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
                "SuggestionOfficialResponseUpdated",
                new
                {
                    suggestionId = evt.SuggestionId,
                    suggestionTitle = evt.SuggestionTitle,
                    updatedAt = evt.UpdatedAt
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
