using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Events;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.AssignDepartment;

public sealed class AssignDepartmentCommandHandler(
    IApplicationDbContext context,
    IPublishEndpoint publishEndpoint,
    IOrganizationDepartmentAssignmentService assignmentService)
    : IRequestHandler<AssignDepartmentCommand, Result<AssignDepartmentCommandResponse>>
{
    public async Task<Result<AssignDepartmentCommandResponse>> Handle(
        AssignDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .Include(candidate => candidate.Neighborhood)
                .ThenInclude(neighborhood => neighborhood.DistrictNavigation)
            .FirstOrDefaultAsync(candidate => candidate.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        OrganizationDepartmentAssignment? assignment = null;
        if (request.OrganizationDepartmentId.HasValue)
        {
            assignment = await assignmentService.GetCompatibleAsync(
                request.OrganizationDepartmentId.Value,
                report.NeighborhoodId,
                cancellationToken)
                ?? throw new ForbiddenAccessException(
                    "Seçilen kurum birimi raporun coğrafi kapsamıyla uyumlu değil.");
        }

        if (report.AssignedOrganizationDepartmentId == request.OrganizationDepartmentId)
        {
            return Result<AssignDepartmentCommandResponse>.Failure(
                ["Rapor zaten belirtilen kurum birimine atanmış durumda."]);
        }

        if (report.Status is not (ReportStatus.Confirmed or ReportStatus.InProgress))
        {
            return Result<AssignDepartmentCommandResponse>.Failure(
                ["Yalnızca onaylanmış veya işleme alınmış raporların birim ataması değiştirilebilir."]);
        }

        var previousStatus = report.Status;
        report.AssignOrganizationDepartment(request.OrganizationDepartmentId);

        if (request.OrganizationDepartmentId.HasValue && report.Status == ReportStatus.Confirmed)
        {
            var history = report.ChangeStatus(
                ReportStatus.InProgress,
                request.ChangedByUserId,
                $"Rapor {assignment!.DepartmentName} birimine atandı ve işleme alındı.");
            if (history is not null)
                context.ReportHistories.Add(history);
        }
        else if (!request.OrganizationDepartmentId.HasValue &&
                 report.Status == ReportStatus.InProgress)
        {
            var history = report.ChangeStatus(
                ReportStatus.Confirmed,
                request.ChangedByUserId,
                "Birim ataması kaldırıldığı için rapor onaylandı durumuna alındı.");
            if (history is not null)
                context.ReportHistories.Add(history);
        }

        await context.SaveChangesAsync(cancellationToken);
        var changedAt = DateTime.UtcNow;

        await publishEndpoint.Publish(new ReportDepartmentChangedEvent
        {
            ReportId = report.Id,
            ReportTitle = report.Title,
            ReportOwnerId = report.UserId,
            ProvinceId = report.Neighborhood.DistrictNavigation.ProvinceId,
            DistrictId = report.Neighborhood.DistrictId,
            NeighborhoodId = report.NeighborhoodId,
            OrganizationDepartmentId = assignment?.OrganizationDepartmentId,
            OrganizationId = assignment?.OrganizationId,
            OrganizationName = assignment?.OrganizationName,
            DepartmentId = assignment?.DepartmentId,
            DepartmentName = assignment?.DepartmentName,
            ChangedAt = changedAt
        }, cancellationToken);

        if (previousStatus != report.Status)
        {
            await publishEndpoint.Publish(new ReportStatusChangedEvent
            {
                ReportId = report.Id,
                ReportTitle = report.Title,
                ReportOwnerId = report.UserId,
                OldStatus = previousStatus,
                NewStatus = report.Status,
                Note = request.OrganizationDepartmentId.HasValue
                    ? $"Rapor {assignment!.DepartmentName} birimine atandı."
                    : "Raporun birim ataması kaldırıldı.",
                ChangedAt = changedAt
            }, cancellationToken);
        }

        var message = request.OrganizationDepartmentId.HasValue
            ? previousStatus == ReportStatus.Confirmed
                ? $"Rapor {assignment!.DepartmentName} birimine atandı ve işleme alındı."
                : $"Rapor {assignment!.DepartmentName} birimine atandı."
            : previousStatus == ReportStatus.InProgress
                ? "Raporun birim ataması kaldırıldı ve durumu onaylandı olarak güncellendi."
                : "Raporun birim ataması kaldırıldı.";

        return Result<AssignDepartmentCommandResponse>.Success(
            new AssignDepartmentCommandResponse(
                report.Id,
                assignment?.OrganizationDepartmentId,
                assignment?.OrganizationId,
                assignment?.OrganizationName,
                assignment?.DepartmentId,
                assignment?.DepartmentName,
                report.Status,
                changedAt,
                message));
    }
}
