using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Users.Queries.GetUsersByProvince;

public sealed class GetUsersByProvinceQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetUsersByProvinceQuery, Result<IReadOnlyList<GetUsersByProvinceQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetUsersByProvinceQueryResponse>>> Handle(
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
            .Select(u => new GetUsersByProvinceQueryResponse(
                u.Id,
                u.FirstName,
                u.LastName,
                u.FirstName + " " + u.LastName,
                u.Role,
                u.Bio,
                u.ProfileImageUrl,
                u.NeighborhoodId,
                u.Neighborhood != null ? u.Neighborhood.Name : null))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetUsersByProvinceQueryResponse>>.Success(users);
    }
}
