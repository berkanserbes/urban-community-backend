using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

public sealed class ReportDepartmentChangedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService)
    : IConsumer<ReportDepartmentChangedEvent>
{
    public async Task Consume(ConsumeContext<ReportDepartmentChangedEvent> consumeContext)
    {
        var evt = consumeContext.Message;
        var notifications = new List<Notification>();

        var ownerMessage = evt.DepartmentId.HasValue
            ? $"\"{evt.ReportTitle}\" başlıklı raporunuz {evt.DepartmentName} birimine atandı."
            : $"\"{evt.ReportTitle}\" başlıklı raporunuzun birim ataması kaldırıldı.";

        notifications.Add(CreateNotification(evt.ReportOwnerId, evt.ReportId, ownerMessage));

        var officialIds = new List<Guid>();

        if (evt.DepartmentId.HasValue)
        {
            officialIds = await context.Users
                .AsNoTracking()
                .Where(u =>
                    u.IsActive &&
                    u.Role == UserRole.Official &&
                    u.DepartmentId == evt.DepartmentId.Value &&
                    u.Id != evt.ReportOwnerId)
                .Select(u => u.Id)
                .ToListAsync(consumeContext.CancellationToken);

            notifications.AddRange(officialIds.Select(userId =>
                CreateNotification(
                    userId,
                    evt.ReportId,
                    $"Biriminize yeni bir rapor atandı: \"{evt.ReportTitle}\"")));
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(consumeContext.CancellationToken);

        var ownerEventName = evt.DepartmentId.HasValue
            ? "ReportDepartmentAssigned"
            : "ReportDepartmentUnassigned";

        await notificationService.SendToUserAsync(
            evt.ReportOwnerId.ToString(),
            ownerEventName,
            new
            {
                reportId = evt.ReportId,
                reportTitle = evt.ReportTitle,
                departmentId = evt.DepartmentId,
                departmentName = evt.DepartmentName,
                changedAt = evt.ChangedAt
            },
            consumeContext.CancellationToken);

        if (officialIds.Count == 0)
            return;

        var officialData = new
        {
            reportId = evt.ReportId,
            reportTitle = evt.ReportTitle,
            departmentId = evt.DepartmentId,
            departmentName = evt.DepartmentName,
            changedAt = evt.ChangedAt
        };

        await Task.WhenAll(officialIds.Select(userId =>
            notificationService.SendToUserAsync(
                userId.ToString(),
                "ReportDepartmentAssigned",
                officialData,
                consumeContext.CancellationToken)));
    }

    private static Notification CreateNotification(Guid userId, Guid reportId, string message)
    {
        return new Notification
        {
            UserId = userId,
            Title = "Rapor Birimi Güncellendi",
            Message = message,
            IsRead = false,
            RelatedEntityId = reportId,
            RelatedEntityType = NotificationRelatedEntityType.Report
        };
    }
}
