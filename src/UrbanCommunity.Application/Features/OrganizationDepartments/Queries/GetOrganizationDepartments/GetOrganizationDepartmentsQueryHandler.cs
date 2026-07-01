using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.OrganizationDepartments.Queries.GetOrganizationDepartments;

public sealed class GetOrganizationDepartmentsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<
        GetOrganizationDepartmentsQuery,
        Result<IReadOnlyList<GetOrganizationDepartmentsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetOrganizationDepartmentsQueryResponse>>> Handle(
        GetOrganizationDepartmentsQuery request,
        CancellationToken cancellationToken)
    {
        var organizationExists = await context.Organizations
            .AsNoTracking()
            .AnyAsync(
                organization => organization.Id == request.OrganizationId,
                cancellationToken);
        if (!organizationExists)
            throw new NotFoundException(nameof(Organization), request.OrganizationId);

        var query = context.OrganizationDepartments
            .AsNoTracking()
            .Where(value => value.OrganizationId == request.OrganizationId);
        if (request.IsActive.HasValue)
            query = query.Where(value => value.IsActive == request.IsActive.Value);

        var departments = await query
            .OrderBy(value => value.Department.Name)
            .Select(value => new GetOrganizationDepartmentsQueryResponse(
                value.Id,
                value.OrganizationId,
                value.Organization.Name,
                value.DepartmentId,
                value.Department.Name,
                value.DisplayName ?? value.Department.Name,
                value.ContactEmail,
                value.ContactPhone,
                value.IsActive,
                value.Categories.Select(link => link.ReportCategoryId).ToList(),
                value.Officials.Count,
                value.AssignedReports.Count))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetOrganizationDepartmentsQueryResponse>>.Success(
            departments);
    }
}
