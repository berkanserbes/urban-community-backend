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
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<ChangeReportStatusCommand, Result<ChangeReportStatusCommandResponse>>
{
    public async Task<Result<ChangeReportStatusCommandResponse>> Handle(ChangeReportStatusCommand request, CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        var oldStatus = report.Status;

        if (oldStatus == request.NewStatus)
            return Result<ChangeReportStatusCommandResponse>.Failure(["Rapor zaten belirtilen durumda."]);

        if (request.NewStatus == ReportStatus.InProgress && !report.AssignedDepartmentId.HasValue)
        {
            return Result<ChangeReportStatusCommandResponse>.Failure(
                ["Rapor işleme alınmadan önce bir birime atanmalıdır."]);
        }

        report.ChangeStatus(request.NewStatus, request.ChangedByUserId, request.Note);

        await context.SaveChangesAsync(cancellationToken);

        var changedAt = DateTime.UtcNow;

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
