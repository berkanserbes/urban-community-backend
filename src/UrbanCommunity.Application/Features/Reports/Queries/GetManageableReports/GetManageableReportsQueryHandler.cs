using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Features.Reports.Queries.GetReports;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;

public sealed class GetManageableReportsQueryHandler(
    IApplicationDbContext context,
    IReportManagementAuthorizationService authorizationService)
    : IRequestHandler<GetManageableReportsQuery, Result<IReadOnlyList<GetReportsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetReportsQueryResponse>>> Handle(
        GetManageableReportsQuery request,
        CancellationToken cancellationToken)
    {
        var access = await authorizationService.GetAccessAsync(
            request.RequestingUserId,
            request.IsAdmin,
            cancellationToken);

        var query = authorizationService.ApplyManageableScope(
            context.Reports.AsNoTracking(),
            access);

        if (request.NeighborhoodId.HasValue)
            query = query.Where(report => report.NeighborhoodId == request.NeighborhoodId.Value);

        if (request.Status.HasValue)
            query = query.Where(report => report.Status == request.Status.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(report => report.CategoryId == request.CategoryId.Value);

        if (request.AssignedOrganizationDepartmentId.HasValue)
            query = query.Where(report =>
                report.AssignedOrganizationDepartmentId ==
                request.AssignedOrganizationDepartmentId.Value);

        var reports = await query
            .OrderByDescending(report => report.CreatedAt)
            .Select(report => new GetReportsQueryResponse(
                report.Id,
                report.Title,
                report.Description,
                report.Status,
                report.Priority,
                report.Location != null ? report.Location.Y : null,
                report.Location != null ? report.Location.X : null,
                report.UserId,
                report.User.FirstName + " " + report.User.LastName,
                report.CategoryId,
                report.Category.Name,
                report.NeighborhoodId,
                report.Neighborhood.Name,
                report.AssignedOrganizationDepartmentId,
                report.AssignedOrganizationDepartment != null
                    ? report.AssignedOrganizationDepartment.OrganizationId
                    : null,
                report.AssignedOrganizationDepartment != null
                    ? report.AssignedOrganizationDepartment.Organization.Name
                    : null,
                report.AssignedOrganizationDepartment != null
                    ? report.AssignedOrganizationDepartment.DepartmentId
                    : null,
                report.AssignedOrganizationDepartment != null
                    ? report.AssignedOrganizationDepartment.DisplayName ??
                      report.AssignedOrganizationDepartment.Department.Name
                    : null,
                report.AssignedOrganizationDepartment != null
                    ? report.AssignedOrganizationDepartment.ContactEmail
                    : null,
                report.AssignedOrganizationDepartment != null
                    ? report.AssignedOrganizationDepartment.ContactPhone
                    : null,
                report.AssignmentNeedsReview,
                report.CreatedAt,
                report.ResolvedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetReportsQueryResponse>>.Success(reports);
    }
}
