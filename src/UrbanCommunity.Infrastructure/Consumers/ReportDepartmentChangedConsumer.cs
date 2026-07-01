using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Services;

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

        var ownerMessage = evt.OrganizationDepartmentId.HasValue
            ? $"\"{evt.ReportTitle}\" başlıklı raporunuz {evt.OrganizationName} / {evt.DepartmentName} birimine atandı."
            : $"\"{evt.ReportTitle}\" başlıklı raporunuzun birim ataması kaldırıldı.";

        notifications.Add(CreateNotification(evt.ReportOwnerId, evt.ReportId, ownerMessage));
        var officialIds = new List<Guid>();

        if (evt.OrganizationDepartmentId.HasValue)
        {
            officialIds = await context.Users
                .AsNoTracking()
                .Where(user =>
                    user.IsActive &&
                    user.Role == UserRole.Official &&
                    user.OrganizationDepartmentId == evt.OrganizationDepartmentId.Value &&
                    user.Id != evt.ReportOwnerId)
                .WhereJurisdictionMatches(
                    evt.ProvinceId,
                    evt.DistrictId,
                    evt.NeighborhoodId)
                .Select(user => user.Id)
                .ToListAsync(consumeContext.CancellationToken);

            notifications.AddRange(officialIds.Select(userId =>
                CreateNotification(
                    userId,
                    evt.ReportId,
                    $"Biriminize yeni bir rapor atandı: \"{evt.ReportTitle}\"")));
        }

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync(consumeContext.CancellationToken);

        var ownerEventName = evt.OrganizationDepartmentId.HasValue
            ? "ReportDepartmentAssigned"
            : "ReportDepartmentUnassigned";
        var eventData = new
        {
            reportId = evt.ReportId,
            reportTitle = evt.ReportTitle,
            organizationDepartmentId = evt.OrganizationDepartmentId,
            organizationId = evt.OrganizationId,
            organizationName = evt.OrganizationName,
            departmentId = evt.DepartmentId,
            departmentName = evt.DepartmentName,
            changedAt = evt.ChangedAt
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
                n.UserId == evt.ReportOwnerId ? ownerEventName : "ReportDepartmentAssigned",
                eventData,
                consumeContext.CancellationToken)
        }));
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
