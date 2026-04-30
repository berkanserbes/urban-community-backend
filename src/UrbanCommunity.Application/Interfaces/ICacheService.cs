namespace UrbanCommunity.Application.Interfaces;

public record CacheOptions(
    TimeSpan? SlidingExpireTime = null,
    TimeSpan? AbsoluteExpireTime = null
);

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the value from the cache if it exists; otherwise executes the factory,
    /// stores the result in the cache, and returns it.
    /// Encapsulates the cache-aside pattern in a single method.
    /// </summary>
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        CacheOptions? options = null,
        CancellationToken cancellationToken = default);

    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default);
}
