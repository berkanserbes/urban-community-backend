using MassTransit;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Consumers;

/// <summary>
/// Rapor durumu değiştiğinde raporu açan kullanıcıya bildirim gönderir.
/// </summary>
public sealed class ReportStatusChangedConsumer(
    IApplicationDbContext context,
    IRealtimeNotificationService notificationService) : IConsumer<ReportStatusChangedEvent>
{
    public async Task Consume(ConsumeContext<ReportStatusChangedEvent> consumeContext)
    {
        var evt = consumeContext.Message;

        var statusLabel = evt.NewStatus switch
        {
            ReportStatus.PendingReview => "Onay Bekliyor",
            ReportStatus.AiReview      => "Yapay Zeka İncelemesinde",
            ReportStatus.Confirmed     => "Onaylandı",
            ReportStatus.InProgress    => "İşleme Alındı",
            ReportStatus.Resolved      => "Çözüldü",
            ReportStatus.Rejected      => "Reddedildi",
            _                          => evt.NewStatus.ToString()
        };

        var notification = new Notification
        {
            UserId = evt.ReportOwnerId,
            Title = "Rapor Durumu Güncellendi",
            Message = $"\"{evt.ReportTitle}\" başlıklı raporunuzun durumu \"{statusLabel}\" olarak güncellendi.",
            IsRead = false,
            RelatedEntityId = evt.ReportId,
            RelatedEntityType = NotificationRelatedEntityType.Report
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(consumeContext.CancellationToken);

        await notificationService.SendToUserAsync(
            evt.ReportOwnerId.ToString(),
            "ReportStatusChanged",
            new
            {
                reportId = evt.ReportId,
                reportTitle = evt.ReportTitle,
                oldStatus = evt.OldStatus.ToString(),
                newStatus = evt.NewStatus.ToString(),
                note = evt.Note,
                changedAt = evt.ChangedAt
            },
            consumeContext.CancellationToken);
    }
}
