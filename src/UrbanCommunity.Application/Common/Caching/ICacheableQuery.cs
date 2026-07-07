using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Common.Caching;

public interface ICacheableQuery
{
    string CacheKey { get; }
    CacheOptions CacheOptions { get; }
}
