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
    IPublishEndpoint publishEndpoint)
    : IRequestHandler<AssignDepartmentCommand, Result<AssignDepartmentCommandResponse>>
{
    public async Task<Result<AssignDepartmentCommandResponse>> Handle(
        AssignDepartmentCommand request,
        CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .Include(candidate => candidate.Neighborhood)
                .ThenInclude(neighborhood => neighborhood.DistrictNavigation)
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        Department? department = null;

        if (request.DepartmentId.HasValue)
        {
            department = await context.Departments
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == request.DepartmentId.Value, cancellationToken)
                ?? throw new NotFoundException(nameof(Department), request.DepartmentId.Value);
        }

        if (report.AssignedDepartmentId == request.DepartmentId)
            return Result<AssignDepartmentCommandResponse>.Failure(
                ["Rapor zaten belirtilen birime atanmış durumda."]);

        if (report.Status is not (ReportStatus.Confirmed or ReportStatus.InProgress))
        {
            return Result<AssignDepartmentCommandResponse>.Failure(
                ["Yalnızca onaylanmış veya işleme alınmış raporların birim ataması değiştirilebilir."]);
        }

        var previousStatus = report.Status;

        report.AssignDepartment(request.DepartmentId);

        if (request.DepartmentId.HasValue && report.Status == ReportStatus.Confirmed)
        {
            report.ChangeStatus(
                ReportStatus.InProgress,
                request.ChangedByUserId,
                $"Rapor {department!.Name} birimine atandı ve işleme alındı.");
        }
        else if (!request.DepartmentId.HasValue && report.Status == ReportStatus.InProgress)
        {
            report.ChangeStatus(
                ReportStatus.Confirmed,
                request.ChangedByUserId,
                "Birim ataması kaldırıldığı için rapor onaylandı durumuna alındı.");
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
            DepartmentId = department?.Id,
            DepartmentName = department?.Name,
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
                Note = request.DepartmentId.HasValue
                    ? $"Rapor {department!.Name} birimine atandı."
                    : "Raporun birim ataması kaldırıldı.",
                ChangedAt = changedAt
            }, cancellationToken);
        }

        var message = request.DepartmentId.HasValue
            ? previousStatus == ReportStatus.Confirmed
                ? $"Rapor {department!.Name} birimine atandı ve işleme alındı."
                : $"Rapor {department!.Name} birimine atandı."
            : previousStatus == ReportStatus.InProgress
                ? "Raporun birim ataması kaldırıldı ve durumu onaylandı olarak güncellendi."
                : "Raporun birim ataması kaldırıldı.";

        return Result<AssignDepartmentCommandResponse>.Success(
            new AssignDepartmentCommandResponse(
                report.Id,
                department?.Id,
                department?.Name,
                report.Status,
                changedAt,
                message));
    }
}
