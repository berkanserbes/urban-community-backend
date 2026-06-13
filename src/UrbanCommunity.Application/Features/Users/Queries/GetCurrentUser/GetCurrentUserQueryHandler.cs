using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetCurrentUser;

public sealed class GetCurrentUserQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetCurrentUserQuery, Result<GetCurrentUserQueryResponse>>
{
    public async Task<Result<GetCurrentUserQueryResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == request.UserId)
            .Select(u => new GetCurrentUserQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Email!,
                u.PhoneNumber,
                u.Role,
                u.IsActive,
                u.Neighborhood != null && u.Neighborhood.DistrictNavigation != null ? u.Neighborhood.DistrictNavigation.ProvinceId : null,
                u.Neighborhood != null && u.Neighborhood.DistrictNavigation != null && u.Neighborhood.DistrictNavigation.Province != null ? u.Neighborhood.DistrictNavigation.Province.Name : null,
                u.Neighborhood != null ? u.Neighborhood.DistrictId : null,
                u.Neighborhood != null && u.Neighborhood.DistrictNavigation != null ? u.Neighborhood.DistrictNavigation.Name : null,
                u.NeighborhoodId,
                u.Neighborhood != null ? u.Neighborhood.Name : null,
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
                u.CreatedAt,
                u.UpdatedAt))
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        return Result<GetCurrentUserQueryResponse>.Success(user);
    }
}
