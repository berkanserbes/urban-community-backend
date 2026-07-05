using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed class GetReportsQueryHandler(
    IApplicationDbContext context,
    IReportManagementAuthorizationService authorizationService)
    : IRequestHandler<GetReportsQuery, Result<IReadOnlyList<GetReportsQueryResponse>>>
{
public async Task<Result<IReadOnlyList<GetReportsQueryResponse>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Reports
            .AsNoTracking()
            .AsQueryable();

        if (request.RequestingUserId.HasValue && request.IsOfficial)
        {
            var access = await authorizationService.GetAccessAsync(
                request.RequestingUserId.Value,
                false,
                cancellationToken);

            query = authorizationService.ApplyManageableScope(query, access);
        }
        else if (request.RequestingUserId.HasValue)
        {
            var location = await context.Users
                .Where(u => u.Id == request.RequestingUserId.Value)
                .Select(u => new
                {
                    u.NeighborhoodId,
                    DistrictId = u.Neighborhood != null
                        ? u.Neighborhood.DistrictId
                        : (Guid?)null,
                    ProvinceId = u.Neighborhood != null
                        ? u.Neighborhood.DistrictNavigation.ProvinceId
                        : (Guid?)null,
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (location?.NeighborhoodId is not null)
            {
                var scope = request.Scope ?? "Neighborhood";
                query = scope switch
                {
                    "District" when location.DistrictId.HasValue =>
                        query.Where(r => r.Neighborhood.DistrictId == location.DistrictId.Value),
                    "Province" when location.ProvinceId.HasValue =>
                        query.Where(r => r.Neighborhood.DistrictNavigation.ProvinceId == location.ProvinceId.Value),
                    _ => query.Where(r => r.NeighborhoodId == location.NeighborhoodId.Value),
                };
            }
        }

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        if (request.CategoryId.HasValue)
            query = query.Where(r => r.CategoryId == request.CategoryId.Value);

        if (request.AssignedOrganizationDepartmentId.HasValue)
            query = query.Where(r =>
                r.AssignedOrganizationDepartmentId == request.AssignedOrganizationDepartmentId.Value);

        if (request.UserId.HasValue)
            query = query.Where(r => r.UserId == request.UserId.Value);

        var reports = await query
            .OrderByDescending(r => r.CreatedAt)
            .Select(r => new GetReportsQueryResponse(
                r.Id,
                r.Title,
                r.Description,
                r.Status,
                r.Priority,
                r.Location != null ? r.Location.Y : null,
                r.Location != null ? r.Location.X : null,
                r.UserId,
                r.User.FirstName + " " + r.User.LastName,
                r.CategoryId,
                r.Category.Name,
                r.NeighborhoodId,
                r.Neighborhood.Name,
                r.AssignedOrganizationDepartmentId,
                r.AssignedOrganizationDepartment != null
                    ? r.AssignedOrganizationDepartment.OrganizationId
                    : null,
                r.AssignedOrganizationDepartment != null
                    ? r.AssignedOrganizationDepartment.Organization.Name
                    : null,
                r.AssignedOrganizationDepartment != null
                    ? r.AssignedOrganizationDepartment.DepartmentId
                    : null,
                r.AssignedOrganizationDepartment != null
                    ? r.AssignedOrganizationDepartment.DisplayName ??
                      r.AssignedOrganizationDepartment.Department.Name
                    : null,
                r.AssignedOrganizationDepartment != null
                    ? r.AssignedOrganizationDepartment.ContactEmail
                    : null,
                r.AssignedOrganizationDepartment != null
                    ? r.AssignedOrganizationDepartment.ContactPhone
                    : null,
                r.AssignmentNeedsReview,
                r.SimilarReportMatches
                    .Select(match => match.SimilarReportId)
                    .Concat(r.ReferencedAsSimilarMatches.Select(match => match.ReportId))
                    .Distinct()
                    .Count(),
                r.CreatedAt,
                r.ResolvedAt,
                r.Media
                    .Where(m => m.MediaType == MediaType.Image)
                    .OrderBy(m => m.DisplayOrder)
                    .Select(m => m.ThumbnailUrl ?? m.Url)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetReportsQueryResponse>>.Success(reports);
    }
}
