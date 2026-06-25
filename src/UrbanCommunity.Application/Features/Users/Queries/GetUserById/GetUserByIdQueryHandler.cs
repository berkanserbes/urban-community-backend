using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUserById;

public sealed class GetUserByIdQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUserByIdQuery, Result<GetUserByIdQueryResponse>>
{
    public async Task<Result<GetUserByIdQueryResponse>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.Id)
            .Select(u => new GetUserByIdQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email!,
                u.Role,
                u.IsActive,
                u.NeighborhoodId,
                u.Neighborhood != null ? u.Neighborhood.Name : null,
                u.OrganizationDepartmentId,
                u.OrganizationDepartment != null ? u.OrganizationDepartment.OrganizationId : null,
                u.OrganizationDepartment != null ? u.OrganizationDepartment.Organization.Name : null,
                u.OrganizationDepartment != null ? u.OrganizationDepartment.DepartmentId : null,
                u.OrganizationDepartment != null
                    ? u.OrganizationDepartment.DisplayName ?? u.OrganizationDepartment.Department.Name
                    : null,
                u.OfficialJurisdictionLevel,
                u.OfficialProvinceId ?? u.OfficialDistrictId ?? u.OfficialNeighborhoodId,
                u.OfficialJurisdictionLevel == Domain.Enums.OfficialJurisdictionLevel.Province
                    ? u.OfficialProvince != null ? u.OfficialProvince.Name : null
                    : u.OfficialJurisdictionLevel == Domain.Enums.OfficialJurisdictionLevel.District
                        ? u.OfficialDistrict != null ? u.OfficialDistrict.Name : null
                        : u.OfficialJurisdictionLevel == Domain.Enums.OfficialJurisdictionLevel.Neighborhood
                            ? u.OfficialNeighborhood != null ? u.OfficialNeighborhood.Name : null
                            : null,
                u.Bio,
                u.ProfileImageUrl,
                u.MustChangePassword,
                u.CreatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.Id);
        }

        return Result<GetUserByIdQueryResponse>.Success(user);
    }
}
