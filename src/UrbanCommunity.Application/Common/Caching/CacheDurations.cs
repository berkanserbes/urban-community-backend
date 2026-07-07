using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Application.Common.Caching;

public static class CacheDurations
{
    public static readonly CacheOptions ReferenceData = new(
        AbsoluteExpireTime: TimeSpan.FromHours(12));
}
