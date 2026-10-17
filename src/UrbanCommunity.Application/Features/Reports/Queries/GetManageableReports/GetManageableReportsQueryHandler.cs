using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Common.Projections;
using UrbanCommunity.Application.Features.Reports.Queries.GetReports;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetManageableReports;

public sealed class GetManageableReportsQueryHandler(
    IApplicationDbContext context,
    IReportManagementAuthorizationService authorizationService)
    : IRequestHandler<GetManageableReportsQuery, Result<PagedResult<GetReportsQueryResponse>>>
{
    public async Task<Result<PagedResult<GetReportsQueryResponse>>> Handle(
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

        if (request.Priority.HasValue)
            query = query.Where(report => report.Priority == request.Priority.Value);

        if (request.Assignment == ReportAssignmentFilter.Unassigned)
            query = query.Where(report => report.AssignedOrganizationDepartmentId == null);
        else if (request.Assignment == ReportAssignmentFilter.NeedsReview)
            query = query.Where(report => report.AssignmentNeedsReview);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(report =>
                report.Title.ToLower().Contains(search) ||
                report.Category.Name.ToLower().Contains(search) ||
                report.Neighborhood.Name.ToLower().Contains(search));
        }

        var reports = await query
            .OrderByDescending(report => report.CreatedAt)
            .ThenBy(report => report.Id)
            .Select(report => new GetReportsQueryResponse(
                report.Id,
                report.Title,
                report.Description,
                report.Status,
                report.Priority,
                report.Location != null ? report.Location.Y : null,
                report.Location != null ? report.Location.X : null,
                report.UserId,
                UserProjections.FullName.Invoke(report.User),
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
                report.SimilarReportMatches
                    .Select(match => match.SimilarReportId)
                    .Concat(report.ReferencedAsSimilarMatches.Select(match => match.ReportId))
                    .Distinct()
                    .Count(),
                report.CreatedAt,
                report.ResolvedAt,
                report.Media
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.ThumbnailUrl ?? m.Url)
                    .FirstOrDefault()))
            .Expand()
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<GetReportsQueryResponse>>.Success(reports);
    }
}
