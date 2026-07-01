using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Reports.Queries.GetReportAssignmentOptions;

public sealed record GetReportAssignmentOptionsQuery(Guid ReportId)
    : IRequest<Result<IReadOnlyList<GetReportAssignmentOptionsQueryResponse>>>;

public sealed record GetReportAssignmentOptionsQueryResponse(
    Guid OrganizationDepartmentId,
    Guid OrganizationId,
    string OrganizationName,
    Guid DepartmentId,
    string DepartmentName,
    string? ContactEmail,
    string? ContactPhone,
    bool HandlesReportCategory);

public sealed class GetReportAssignmentOptionsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<
        GetReportAssignmentOptionsQuery,
        Result<IReadOnlyList<GetReportAssignmentOptionsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetReportAssignmentOptionsQueryResponse>>> Handle(
        GetReportAssignmentOptionsQuery request,
        CancellationToken cancellationToken)
    {
        var report = await context.Reports
            .AsNoTracking()
            .Where(value => value.Id == request.ReportId)
            .Select(value => new
            {
                value.CategoryId,
                NeighborhoodId = value.NeighborhoodId,
                value.Neighborhood.DistrictId,
                ProvinceId = value.Neighborhood.DistrictNavigation.ProvinceId
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(nameof(Report), request.ReportId);

        var options = await context.OrganizationDepartments
            .AsNoTracking()
            .Where(value =>
                value.IsActive &&
                value.Organization.IsActive &&
                ((value.Organization.JurisdictionLevel ==
                      OrganizationJurisdictionLevel.Neighborhood &&
                  value.Organization.NeighborhoodId == report.NeighborhoodId) ||
                 (value.Organization.JurisdictionLevel ==
                      OrganizationJurisdictionLevel.District &&
                  value.Organization.DistrictId == report.DistrictId) ||
                 (value.Organization.JurisdictionLevel ==
                      OrganizationJurisdictionLevel.Province &&
                  value.Organization.ProvinceId == report.ProvinceId)))
            .OrderByDescending(value => value.Organization.JurisdictionLevel)
            .ThenBy(value => value.Organization.Name)
            .ThenBy(value => value.DisplayName ?? value.Department.Name)
            .Select(value => new GetReportAssignmentOptionsQueryResponse(
                value.Id,
                value.OrganizationId,
                value.Organization.Name,
                value.DepartmentId,
                value.DisplayName ?? value.Department.Name,
                value.ContactEmail,
                value.ContactPhone,
                value.Categories.Any(link => link.ReportCategoryId == report.CategoryId)))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetReportAssignmentOptionsQueryResponse>>.Success(options);
    }
}
