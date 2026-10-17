using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Extensions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;

public sealed class GetUsersByProvinceQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUsersByProvinceQuery, Result<PagedResult<GetUsersByProvinceQueryResponse>>>
{
    public async Task<Result<PagedResult<GetUsersByProvinceQueryResponse>>> Handle(
        GetUsersByProvinceQuery request,
        CancellationToken cancellationToken)
    {
        var provinceExists = await context.Provinces
            .AsNoTracking()
            .AnyAsync(p => p.Id == request.ProvinceId, cancellationToken);

        if (!provinceExists)
            throw new NotFoundException(nameof(Province), request.ProvinceId);

        var users = await context.Users
            .AsNoTracking()
            .Where(u => u.Neighborhood != null
                        && u.Neighborhood.DistrictNavigation != null
                        && u.Neighborhood.DistrictNavigation.ProvinceId == request.ProvinceId
                        && u.IsActive)
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .ThenBy(u => u.Id)
            .Select(u => new GetUsersByProvinceQueryResponse(
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

        return Result<PagedResult<GetUsersByProvinceQueryResponse>>.Success(users);
    }
}
