using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Commands.ChangeReportPriority;

public sealed class ChangeReportPriorityCommandHandler(
    IApplicationDbContext context,
    IReportManagementAuthorizationService authorizationService)
    : IRequestHandler<ChangeReportPriorityCommand, Result<ChangeReportPriorityCommandResponse>>
{
    public async Task<Result<ChangeReportPriorityCommandResponse>> Handle(
        ChangeReportPriorityCommand request,
        CancellationToken cancellationToken)
    {
        await authorizationService.EnsureCanManageAsync(
            request.ReportId,
            request.RequestingUserId,
            request.IsAdmin,
            cancellationToken);

        var report = await context.Reports
            .FirstOrDefaultAsync(r => r.Id == request.ReportId, cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        if (report.Status == ReportStatus.AiReview)
        {
            return Result<ChangeReportPriorityCommandResponse>.Failure(
                ["AI incelemesi devam eden raporun önceliği manuel olarak değiştirilemez."]);
        }

        if (report.Priority == request.Priority)
            return Result<ChangeReportPriorityCommandResponse>.Failure(["Rapor zaten belirtilen önceliğe sahip."]);

        report.UpdatePriority(request.Priority);
        await context.SaveChangesAsync(cancellationToken);

        return Result<ChangeReportPriorityCommandResponse>.Success(
            new ChangeReportPriorityCommandResponse(report.Id, report.Priority, report.UpdatedAt));
    }
}
