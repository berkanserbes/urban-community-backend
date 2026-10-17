using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReportStats;

public sealed class GetManageableReportStatsQueryHandler(
    IApplicationDbContext context,
    IReportManagementAuthorizationService authorizationService)
    : IRequestHandler<GetManageableReportStatsQuery, Result<GetManageableReportStatsQueryResponse>>
{
    public async Task<Result<GetManageableReportStatsQueryResponse>> Handle(
        GetManageableReportStatsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await authorizationService.GetAccessAsync(
            request.RequestingUserId,
            request.IsAdmin,
            cancellationToken);

        var stats = await authorizationService
            .ApplyManageableScope(context.Reports.AsNoTracking(), access)
            .GroupBy(_ => 1)
            .Select(reports => new GetManageableReportStatsQueryResponse(
                reports.Count(),
                reports.Count(report => report.Status == ReportStatus.PendingReview),
                reports.Count(report => report.Status == ReportStatus.AiReview),
                reports.Count(report => report.Status == ReportStatus.Confirmed),
                reports.Count(report => report.Status == ReportStatus.InProgress),
                reports.Count(report => report.Status == ReportStatus.Resolved),
                reports.Count(report => report.Status == ReportStatus.Rejected),
                reports.Count(report => report.Priority == ReportPriority.Urgent),
                reports.Count(report => report.AssignedOrganizationDepartmentId == null),
                reports.Count(report => report.AssignmentNeedsReview)))
            .FirstOrDefaultAsync(cancellationToken);

        return Result<GetManageableReportStatsQueryResponse>.Success(
            stats ?? GetManageableReportStatsQueryResponse.Empty);
    }
}
