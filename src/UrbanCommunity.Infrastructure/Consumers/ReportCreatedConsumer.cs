using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Services;

namespace UrbanCommunity.Infrastructure.Consumers;

/// <summary>
/// Yeni rapor oluşturulduğunda tüm adminlere ve raporun mahallesindeki yetkililere bildirim gönderir.
/// </summary>
public sealed class ReportCreatedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService) : IConsumer<ReportCreatedEvent>
{
    public async Task Consume(ConsumeContext<ReportCreatedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        var adminIds = await context.Users
            .AsNoTracking()
            .Where(u =>
                u.IsActive &&
                u.Role == UserRole.Admin)
            .Select(u => u.Id)
            .ToListAsync(consumeContext.CancellationToken);

        var officialIds = await context.Users
            .AsNoTracking()
            .Where(user =>
                user.IsActive &&
                user.Role == UserRole.Official)
            .WhereJurisdictionMatches(
                evt.ProvinceId,
                evt.DistrictId,
                evt.NeighborhoodId)
            .Select(user => user.Id)
            .ToListAsync(consumeContext.CancellationToken);

        var recipientIds = adminIds
            .Concat(officialIds)
            .Distinct()
            .ToList();

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

        var realtimePayload = new
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
        };

        await Task.WhenAll(recipientIds.Select(userId =>
            notificationService.SendToUserAsync(
                userId.ToString(),
                "NewReport",
                realtimePayload,
                consumeContext.CancellationToken)));
    }
}
