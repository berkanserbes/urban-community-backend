using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public class InMemoryCacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly string _keyPrefix;
    private readonly HashSet<string> _keys = [];
    private readonly Lock _lock = new();

    public InMemoryCacheService(IMemoryCache memoryCache, IConfiguration configuration)
    {
        _memoryCache = memoryCache;
        _keyPrefix = NormalizePrefix(configuration["CacheSettings:KeyPrefix"]);
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.TryGetValue(ApplyPrefix(key), out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
    {
        var entryOptions = new MemoryCacheEntryOptions();

        if (options?.SlidingExpireTime.HasValue == true)
            entryOptions.SetSlidingExpiration(options.SlidingExpireTime.Value);

        if (options?.AbsoluteExpireTime.HasValue == true)
            entryOptions.SetAbsoluteExpiration(options.AbsoluteExpireTime.Value);

        var prefixedKey = ApplyPrefix(key);
        _memoryCache.Set(prefixedKey, value, entryOptions);
        lock (_lock) { _keys.Add(prefixedKey); }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_memoryCache.TryGetValue(ApplyPrefix(key), out _));
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
        var prefixedKey = ApplyPrefix(key);
        _memoryCache.Remove(prefixedKey);
        lock (_lock) { _keys.Remove(prefixedKey); }
        return Task.CompletedTask;
    }

    public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var prefixedPrefix = ApplyPrefix(prefixKey);
        lock (_lock)
        {
            var keysToRemove = _keys
                .Where(k => k.StartsWith(prefixedPrefix, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _memoryCache.Remove(key);
                _keys.Remove(key);
            }
        }

        return Task.CompletedTask;
    }

    private string ApplyPrefix(string key)
    {
        return string.IsNullOrWhiteSpace(_keyPrefix) ? key : $"{_keyPrefix}:{key}";
    }

    private static string NormalizePrefix(string? prefix)
    {
        return string.IsNullOrWhiteSpace(prefix) ? string.Empty : prefix.Trim().TrimEnd(':');
    }
}
