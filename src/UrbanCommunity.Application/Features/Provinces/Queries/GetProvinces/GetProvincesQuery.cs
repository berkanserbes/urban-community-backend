using MediatR;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Features.Provinces.Queries.GetProvinces;

public sealed record GetProvincesQuery
    : IRequest<Result<IReadOnlyList<GetProvincesQueryResponse>>>, ICacheableQuery
{
    public string CacheKey => CacheKeys.ProvincesList;
    public CacheOptions CacheOptions => CacheDurations.ReferenceData;
}
