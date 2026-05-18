using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodsByDistrict;

public sealed class GetNeighborhoodsByDistrictQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNeighborhoodsByDistrictQuery, Result<IReadOnlyList<GetNeighborhoodsByDistrictQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetNeighborhoodsByDistrictQueryResponse>>> Handle(GetNeighborhoodsByDistrictQuery request, CancellationToken cancellationToken)
    {
        var districtExists = await context.Districts
            .AsNoTracking()
            .AnyAsync(d => d.Id == request.DistrictId, cancellationToken);

        if (!districtExists)
            throw new NotFoundException(nameof(District), request.DistrictId);

        var neighborhoods = await context.Neighborhoods
            .AsNoTracking()
            .Where(n => n.DistrictId == request.DistrictId)
            .OrderBy(n => n.Name)
            .Select(n => new GetNeighborhoodsByDistrictQueryResponse(n.Id, n.Name))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetNeighborhoodsByDistrictQueryResponse>>.Success(neighborhoods);
    }
}
