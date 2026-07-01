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

        var assignedOrganizationDepartmentId = await context.Reports
            .AsNoTracking()
            .Where(report => report.Id == evt.ReportId)
            .Select(report => report.AssignedOrganizationDepartmentId)
            .SingleOrDefaultAsync(consumeContext.CancellationToken);

        var officialIds = assignedOrganizationDepartmentId.HasValue
            ? await context.Users
                .AsNoTracking()
                .Where(user =>
                    user.IsActive &&
                    user.Role == UserRole.Official &&
                    user.OrganizationDepartmentId == assignedOrganizationDepartmentId.Value)
                .WhereJurisdictionMatches(
                    evt.ProvinceId,
                    evt.DistrictId,
                    evt.NeighborhoodId)
                .Select(user => user.Id)
                .ToListAsync(consumeContext.CancellationToken)
            : [];

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

        await Task.WhenAll(notifications.SelectMany(n => new[]
        {
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
                consumeContext.CancellationToken),
            notificationService.SendToUserAsync(
                n.UserId.ToString(),
                "NewReport",
                realtimePayload,
                consumeContext.CancellationToken)
        }));
    }
}
