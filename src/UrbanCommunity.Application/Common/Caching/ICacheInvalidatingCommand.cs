namespace UrbanCommunity.Application.Common.Caching;

public interface ICacheInvalidatingCommand
{
    IReadOnlyCollection<string> CacheKeysToRemove { get; }
    IReadOnlyCollection<string> CachePrefixesToRemove { get; }
}
