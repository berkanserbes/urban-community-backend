using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportStatus;

public sealed class ChangeReportStatusCommandHandler(
    IApplicationDbContext context,
    IReportManagementAuthorizationService authorizationService,
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ChangeReportStatusCommand, Result<ChangeReportStatusCommandResponse>>
{
    public async Task<Result<ChangeReportStatusCommandResponse>> Handle(ChangeReportStatusCommand request, CancellationToken cancellationToken)
    {
        await authorizationService.EnsureCanManageAsync(
            request.ReportId,
            request.RequestingUserId,
            request.IsAdmin,
            cancellationToken);

        var report = await context.Reports
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        var oldStatus = report.Status;

        if (oldStatus == ReportStatus.AiReview)
        {
            return Result<ChangeReportStatusCommandResponse>.Failure(
                ["AI incelemesi devam eden raporun durumu manuel olarak değiştirilemez."]);
        }

        if (oldStatus == request.NewStatus)
            return Result<ChangeReportStatusCommandResponse>.Failure(["Rapor zaten belirtilen durumda."]);

        if (request.NewStatus == ReportStatus.InProgress &&
            !report.AssignedOrganizationDepartmentId.HasValue)
        {
            return Result<ChangeReportStatusCommandResponse>.Failure(
                ["Rapor işleme alınmadan önce bir birime atanmalıdır."]);
        }

        var history = report.ChangeStatus(request.NewStatus, request.RequestingUserId, request.Note);
        if (history is not null)
            context.ReportHistories.Add(history);

        var changedAt = DateTime.UtcNow;

        // Published through the transactional outbox: SaveChanges stores it with the data below.
        // The broker is not called here, so a broker outage can no longer fail the request.
        await publishEndpoint.Publish(new ReportStatusChangedEvent
        {
            ReportId = report.Id,
            ReportTitle = report.Title,
            ReportOwnerId = report.UserId,
            OldStatus = oldStatus,
            NewStatus = report.Status,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            ChangedAt = changedAt
        }, cancellationToken);

        await context.SaveChangesAsync(cancellationToken);

        return Result<ChangeReportStatusCommandResponse>.Success(
            new ChangeReportStatusCommandResponse(
                report.Id,
                oldStatus,
                report.Status,
                changedAt,
                BuildStatusChangedMessage(oldStatus, report.Status)));
    }

    private static string BuildStatusChangedMessage(ReportStatus fromStatus, ReportStatus toStatus)
    {
        return $"Rapor durumu \"{GetStatusLabel(fromStatus)}\" durumundan \"{GetStatusLabel(toStatus)}\" durumuna geçti.";
    }

    private static string GetStatusLabel(ReportStatus status)
    {
        return status switch
        {
            ReportStatus.PendingReview => "Onay bekliyor",
            ReportStatus.AiReview => "Yapay zeka incelemesinde",
            ReportStatus.Confirmed => "Onaylandı",
            ReportStatus.InProgress => "İşleme alındı",
            ReportStatus.Resolved => "Çözüldü",
            ReportStatus.Rejected => "Reddedildi",
            _ => status.ToString()
        };
    }
}
