using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodLocation;

public sealed class GetNeighborhoodLocationQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNeighborhoodLocationQuery, Result<GetNeighborhoodLocationQueryResponse>>
{
    public async Task<Result<GetNeighborhoodLocationQueryResponse>> Handle(
        GetNeighborhoodLocationQuery request,
        CancellationToken cancellationToken)
    {
        var location = await context.Neighborhoods
            .AsNoTracking()
            .Where(n => n.Id == request.NeighborhoodId)
            .Select(n => new GetNeighborhoodLocationQueryResponse(
                n.Id,
                n.Name,
                n.DistrictId,
                n.DistrictNavigation != null ? n.DistrictNavigation.Name : null,
                n.DistrictNavigation != null ? (Guid?)n.DistrictNavigation.ProvinceId : null,
                n.DistrictNavigation != null ? n.DistrictNavigation.Province.Name : null))
            .FirstOrDefaultAsync(cancellationToken);

        if (location is null)
            throw new NotFoundException(nameof(Neighborhood), request.NeighborhoodId);

        return Result<GetNeighborhoodLocationQueryResponse>.Success(location);
    }
}
