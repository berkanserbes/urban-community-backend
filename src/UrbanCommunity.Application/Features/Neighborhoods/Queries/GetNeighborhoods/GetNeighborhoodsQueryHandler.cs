using MediatR;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed class GetNeighborhoodsQueryHandler(IApplicationDbContext context)
    : IRequestHandler<GetNeighborhoodsQuery, Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>>
{
    public async Task<Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>> Handle(GetNeighborhoodsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Neighborhoods.AsNoTracking().AsQueryable();

        if (request.ProvinceId.HasValue)
            query = query.Where(n => n.DistrictNavigation != null
                && n.DistrictNavigation.ProvinceId == request.ProvinceId.Value);

        if (request.DistrictId.HasValue)
            query = query.Where(n => n.DistrictId == request.DistrictId.Value);

        var neighborhoods = await query
            .OrderBy(n => n.DistrictNavigation != null ? n.DistrictNavigation.Province.Name : string.Empty)
            .ThenBy(n => n.DistrictNavigation != null ? n.DistrictNavigation.Name : string.Empty)
            .ThenBy(n => n.Name)
            .Select(n => new GetNeighborhoodsQueryResponse(
                n.Id,
                n.Name,
                n.DistrictId,
                n.DistrictNavigation != null ? n.DistrictNavigation.Name : null,
                n.DistrictNavigation != null ? (Guid?)n.DistrictNavigation.ProvinceId : null,
                n.DistrictNavigation != null ? n.DistrictNavigation.Province.Name : null))
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>.Success(neighborhoods);
    }
}
