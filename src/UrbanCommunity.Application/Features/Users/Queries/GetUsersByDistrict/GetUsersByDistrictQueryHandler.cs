using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByDistrict;

public sealed class GetUsersByDistrictQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUsersByDistrictQuery, Result<PagedResult<GetUsersByDistrictQueryResponse>>>
{
    public async Task<Result<PagedResult<GetUsersByDistrictQueryResponse>>> Handle(
        GetUsersByDistrictQuery request,
        CancellationToken cancellationToken)
    {
        var districtExists = await context.Districts
            .AsNoTracking()
            .AnyAsync(d => d.Id == request.DistrictId, cancellationToken);

        if (!districtExists)
            throw new NotFoundException(nameof(District), request.DistrictId);

        var users = await context.Users
            .AsNoTracking()
            .Where(u => u.Neighborhood != null
                        && u.Neighborhood.DistrictId == request.DistrictId
                        && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.Id)
            .Select(u => new GetUsersByDistrictQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Role,
                u.Bio,
                u.ProfileImageUrl,
                u.NeighborhoodId,
                u.Neighborhood != null ? u.Neighborhood.Name : null,
                u.OrganizationDepartmentId,
                u.OrganizationDepartment != null
                    ? u.OrganizationDepartment.OrganizationId
                    : null,
                u.OrganizationDepartment != null
                    ? u.OrganizationDepartment.Organization.Name
                    : null,
                u.OrganizationDepartment != null
                    ? u.OrganizationDepartment.DepartmentId
                    : null,
                u.OrganizationDepartment != null
                    ? u.OrganizationDepartment.DisplayName ??
                      u.OrganizationDepartment.Department.Name
                    : null,
                u.OfficialJurisdictionLevel,
                u.OfficialProvinceId ?? u.OfficialDistrictId ?? u.OfficialNeighborhoodId,
                u.OfficialJurisdictionLevel == Domain.Enums.OfficialJurisdictionLevel.Province
                    ? u.OfficialProvince != null ? u.OfficialProvince.Name : null
                    : u.OfficialJurisdictionLevel == Domain.Enums.OfficialJurisdictionLevel.District
                        ? u.OfficialDistrict != null ? u.OfficialDistrict.Name : null
                        : u.OfficialJurisdictionLevel == Domain.Enums.OfficialJurisdictionLevel.Neighborhood
                            ? u.OfficialNeighborhood != null ? u.OfficialNeighborhood.Name : null
                            : null))
            .ToPagedResultAsync(request.PageIndex, request.PageSize, cancellationToken);

        return Result<PagedResult<GetUsersByDistrictQueryResponse>>.Success(users);
    }
}
