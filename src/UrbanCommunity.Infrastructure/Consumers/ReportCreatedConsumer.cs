using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

/// <summary>
/// Yeni rapor oluşturulduğunda admin ve yetkililere bildirim gönderir.
/// </summary>
public sealed class ReportCreatedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService) : IConsumer<ReportCreatedEvent>
{
    public async Task Consume(ConsumeContext<ReportCreatedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        // Admin ve Official rolündeki kullanıcıları bul
        var recipientIds = await context.Users
            .Where(u => u.IsActive && (u.Role == UserRole.Admin || u.Role == UserRole.Official))
            .Select(u => u.Id)
            .ToListAsync(consumeContext.CancellationToken);

        var notifications = recipientIds.Select(userId => new Notification
        {
            UserId = userId,
            Title = "Yeni Rapor",
            Message = $"\"{evt.ReportTitle}\" başlıklı yeni bir {evt.CategoryName} raporu oluşturuldu.",
            IsRead = false,
            RelatedEntityId = evt.ReportId,
            RelatedEntityType = NotificationRelatedEntityType.Report
        }).ToList();

        if (notifications.Count > 0)
        {
            context.Notifications.AddRange(notifications);
            await context.SaveChangesAsync(consumeContext.CancellationToken);
        }

        // SignalR ile mahalle grubuna anlık push
        await notificationService.SendToGroupAsync(
            $"neighborhood_{evt.NeighborhoodId}",
            "NewReport",
            new
            {
                reportId = evt.ReportId,
                title = evt.ReportTitle,
                categoryId = evt.CategoryId,
                categoryName = evt.CategoryName,
                categoryAssignedByAi = evt.CategoryAssignedByAi,
                aiCategoryConfidence = evt.AiCategoryConfidence,
                priority = evt.Priority,
                priorityAssignedByAi = evt.PriorityAssignedByAi,
                aiPriorityScore = evt.AiPriorityScore,
                createdAt = evt.CreatedAt
            },
            consumeContext.CancellationToken);
    }
}
