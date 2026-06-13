using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsers;

public sealed class GetUsersQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUsersQuery, Result<IReadOnlyList<GetUsersQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetUsersQueryResponse>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await context.Users
            .AsNoTracking()
            .OrderBy(u => u.FirstName)
            .ThenBy(u => u.LastName)
            .Select(u => new GetUsersQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email!,
                u.Role,
                u.IsActive,
                u.NeighborhoodId,
                u.DepartmentId,
                u.Department != null ? u.Department.Name : null,
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
                u.CreatedAt))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetUsersQueryResponse>>.Success(users);
    }
}
