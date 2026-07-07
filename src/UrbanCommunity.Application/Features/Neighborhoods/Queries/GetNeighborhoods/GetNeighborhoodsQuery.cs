using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoods;

public sealed record GetNeighborhoodsQuery(
    Guid? ProvinceId = null,
    Guid? DistrictId = null) : IRequest<Result<IReadOnlyList<GetNeighborhoodsQueryResponse>>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.NeighborhoodsList(ProvinceId, DistrictId);
    public CacheOptions CacheOptions => CacheDurations.ReferenceData;
}
