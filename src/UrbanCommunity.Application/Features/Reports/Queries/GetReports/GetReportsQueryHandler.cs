using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReports;

public sealed class GetReportsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetReportsQuery, Result<IReadOnlyList<GetReportsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetReportsQueryResponse>>> Handle(GetReportsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Reports
            .AsNoTracking()
            .AsQueryable();

        if (request.NeighborhoodId.HasValue)
            query = query.Where(r => r.NeighborhoodId == request.NeighborhoodId.Value);

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
                r.CreatedAt,
                r.ResolvedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetReportsQueryResponse>>.Success(reports);
    }
}
