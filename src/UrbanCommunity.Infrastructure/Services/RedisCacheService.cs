using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;
using System.Text.Json;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services;

public class RedisCacheService : ICacheService
{
    private const int PrefixDeleteBatchSize = 500;

    private readonly IDistributedCache _distributedCache;
    private readonly IConnectionMultiplexer _connectionMultiplexer;
    private readonly string _keyPrefix;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConnectionMultiplexer connectionMultiplexer,
        IConfiguration configuration)
    {
        _distributedCache = distributedCache;
        _connectionMultiplexer = connectionMultiplexer;
        _keyPrefix = NormalizePrefix(configuration["CacheSettings:KeyPrefix"]);
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var cachedData = await _distributedCache.GetStringAsync(ApplyPrefix(key), cancellationToken);

        if (string.IsNullOrEmpty(cachedData))
        {
            return default;
        }

        return JsonSerializer.Deserialize<T>(cachedData);
    }

    public async Task SetAsync<T>(string key, T value, CacheOptions? options = null, CancellationToken cancellationToken = default)
    {
        var entryOptions = new DistributedCacheEntryOptions();

        if (options?.SlidingExpireTime.HasValue == true)
        {
            entryOptions.SetSlidingExpiration(options.SlidingExpireTime.Value);
        }

        if (options?.AbsoluteExpireTime.HasValue == true)
        {
            entryOptions.SetAbsoluteExpiration(options.AbsoluteExpireTime.Value);
        }

        var serializedData = JsonSerializer.Serialize(value);
        await _distributedCache.SetStringAsync(ApplyPrefix(key), serializedData, entryOptions, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        var value = await _distributedCache.GetAsync(ApplyPrefix(key), cancellationToken);
        return value is not null;
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<Task<T>> factory, CacheOptions? options = null, CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached is not null)
        {
            return cached;
        }

        var value = await factory();
        await SetAsync(key, value, options, cancellationToken);
        return value;
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        await _distributedCache.RemoveAsync(ApplyPrefix(key), cancellationToken);
    }

    public async Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
    {
        var prefixedPrefix = ApplyPrefix(prefixKey);
        var endpoints = _connectionMultiplexer.GetEndPoints();
        var db = _connectionMultiplexer.GetDatabase();

        foreach (var endpoint in endpoints)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var server = _connectionMultiplexer.GetServer(endpoint);
            if (!server.IsConnected)
            {
                continue;
            }

            var batch = new List<RedisKey>(PrefixDeleteBatchSize);
            foreach (var key in server.Keys(
                         database: db.Database,
                         pattern: $"{prefixedPrefix}*",
                         pageSize: PrefixDeleteBatchSize))
            {
                cancellationToken.ThrowIfCancellationRequested();

                batch.Add(key);
                if (batch.Count < PrefixDeleteBatchSize)
                {
                    continue;
                }

                await db.KeyDeleteAsync(batch.ToArray());
                batch.Clear();
            }

            if (batch.Count > 0)
            {
                await db.KeyDeleteAsync(batch.ToArray());
            }
        }
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
