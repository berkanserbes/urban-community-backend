using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Neighborhoods.Queries.GetNeighborhoodsByDistrict;

public sealed record GetNeighborhoodsByDistrictQuery(Guid DistrictId)
    : IRequest<Result<IReadOnlyList<GetNeighborhoodsByDistrictQueryResponse>>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.NeighborhoodsList(null, DistrictId);
    public CacheOptions CacheOptions => CacheDurations.ReferenceData;
}
