using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetDistrictsByProvince;

public sealed record GetDistrictsByProvinceQuery(Guid ProvinceId)
    : IRequest<Result<IReadOnlyList<GetDistrictsByProvinceQueryResponse>>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.DistrictsByProvince(ProvinceId);
    public CacheOptions CacheOptions => CacheDurations.ReferenceData;
}
