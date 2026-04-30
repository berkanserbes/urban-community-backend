using Microsoft.Extensions.Caching.Memory;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly HashSet<string> _keys = [];
    private readonly Lock _lock = new();

    public InMemoryCacheService(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
    {
        var entryOptions = new MemoryCacheEntryOptions();

        if (options?.SlidingExpireTime.HasValue == true)
            entryOptions.SetSlidingExpiration(options.SlidingExpireTime.Value);

        if (options?.AbsoluteExpireTime.HasValue == true)
            entryOptions.SetAbsoluteExpiration(options.AbsoluteExpireTime.Value);

        _memoryCache.Set(key, value, entryOptions);
        lock (_lock) { _keys.Add(key); }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(key, out _));
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
            return cached;

        var value = await factory();
        await SetAsync(key, value, options, cancellationToken);
        return value;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        lock (_lock) { _keys.Remove(key); }
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        List<string> keysToRemove;
        lock (_lock)
        {
            keysToRemove = _keys
                .Where(k => k.StartsWith(prefixKey, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        lock (_lock)
        {
            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keys.Remove(key);
            }
        }

        return Task.CompletedTask;
    }
}
