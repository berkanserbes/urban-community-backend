using MediatR;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using UrbanCommunity.Application.Behaviors;
using UrbanCommunity.Application.Common.Caching;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Application.Interfaces;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Caching;

public sealed class CacheBehaviorTests
{
    [Fact]
    public async Task CachingBehavior_ReturnsCachedResponse_WhenCacheHasValue()
    {
        var cache = new FakeCacheService();
        var request = new TestCacheableQuery("v1:ref:categories:list");
        var cachedResponse = Result<string>.Success("cached");
        await cache.SetAsync(request.CacheKey, cachedResponse);

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(
            cache,
            new TestLogger<CachingBehavior<TestCacheableQuery, Result<string>>>());

        var handlerWasCalled = false;

        var response = await behavior.Handle(
            request,
            _ =>
            {
                handlerWasCalled = true;
                return Task.FromResult(Result<string>.Success("database"));
            },
            CancellationToken.None);

        Assert.False(handlerWasCalled);
        Assert.True(response.IsSuccess);
        Assert.Equal("cached", response.Data);
    }

    [Fact]
    public async Task CachingBehavior_StoresSuccessfulResponse_WhenCacheMisses()
    {
        var cache = new FakeCacheService();
        var request = new TestCacheableQuery("v1:ref:departments:list");

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(
            cache,
            new TestLogger<CachingBehavior<TestCacheableQuery, Result<string>>>());

        var response = await behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("database")),
            CancellationToken.None);

        var cachedResponse = await cache.GetAsync<Result<string>>(request.CacheKey);

        Assert.True(response.IsSuccess);
        Assert.Equal("database", response.Data);
        Assert.NotNull(cachedResponse);
        Assert.Equal("database", cachedResponse.Data);
    }

    [Fact]
    public async Task CachingBehavior_CallsHandler_WhenRequestIsNotCacheable()
    {
        var cache = new FakeCacheService();
        var request = new TestPlainQuery();

        var behavior = new CachingBehavior<TestPlainQuery, Result<string>>(
            cache,
            new TestLogger<CachingBehavior<TestPlainQuery, Result<string>>>());

        var handlerWasCalled = false;

        var response = await behavior.Handle(
            request,
            _ =>
            {
                handlerWasCalled = true;
                return Task.FromResult(Result<string>.Success("database"));
            },
            CancellationToken.None);

        Assert.True(handlerWasCalled);
        Assert.True(response.IsSuccess);
        Assert.Equal("database", response.Data);
    }

    [Fact]
    public async Task CachingBehavior_ContinuesToHandler_WhenCacheReadFails()
    {
        var cache = new FakeCacheService { ThrowOnGet = true };
        var request = new TestCacheableQuery("v1:ref:categories:list");

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(
            cache,
            new TestLogger<CachingBehavior<TestCacheableQuery, Result<string>>>());

        var response = await behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("database")),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("database", response.Data);
    }

    [Fact]
    public async Task CachingBehavior_ReturnsResponse_WhenCacheWriteFails()
    {
        var cache = new FakeCacheService { ThrowOnSet = true };
        var request = new TestCacheableQuery("v1:ref:categories:list");

        var behavior = new CachingBehavior<TestCacheableQuery, Result<string>>(
            cache,
            new TestLogger<CachingBehavior<TestCacheableQuery, Result<string>>>());

        var response = await behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("database")),
            CancellationToken.None);

        Assert.True(response.IsSuccess);
        Assert.Equal("database", response.Data);
    }

    [Fact]
    public async Task CachingBehavior_DoesNotCacheFailedNonGenericResult()
    {
        var cache = new FakeCacheService();
        var request = new TestNonGenericCacheableQuery("v1:ref:categories:list");

        var behavior = new CachingBehavior<TestNonGenericCacheableQuery, Result>(
            cache,
            new TestLogger<CachingBehavior<TestNonGenericCacheableQuery, Result>>());

        var response = await behavior.Handle(
            request,
            _ => Task.FromResult(Result.Failure(["failed"])),
            CancellationToken.None);

        Assert.False(response.IsSuccess);
        Assert.False(await cache.ExistsAsync(request.CacheKey));
    }

    [Fact]
    public async Task CacheInvalidationBehavior_RemovesKeysAndPrefixes_WhenCommandSucceeds()
    {
        var cache = new FakeCacheService();
        await cache.SetAsync("v1:ref:categories:list", Result<string>.Success("cached"));
        await cache.SetAsync("v1:ref:neighborhoods:list:province-id:all:district-id:all", Result<string>.Success("cached"));

        var request = new TestInvalidatingCommand(
            ["v1:ref:categories:list"],
            ["v1:ref:neighborhoods:list"]);

        var behavior = new CacheInvalidationBehavior<TestInvalidatingCommand, Result<string>>(
            cache,
            new TestLogger<CacheInvalidationBehavior<TestInvalidatingCommand, Result<string>>>());

        await behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("ok")),
            CancellationToken.None);

        Assert.False(await cache.ExistsAsync("v1:ref:categories:list"));
        Assert.False(await cache.ExistsAsync("v1:ref:neighborhoods:list:province-id:all:district-id:all"));
    }

    [Fact]
    public async Task CacheInvalidationBehavior_KeepsCache_WhenCommandFails()
    {
        var cache = new FakeCacheService();
        await cache.SetAsync("v1:ref:categories:list", Result<string>.Success("cached"));

        var request = new TestInvalidatingCommand(["v1:ref:categories:list"], []);

        var behavior = new CacheInvalidationBehavior<TestInvalidatingCommand, Result<string>>(
            cache,
            new TestLogger<CacheInvalidationBehavior<TestInvalidatingCommand, Result<string>>>());

        await behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Failure(["failed"])),
            CancellationToken.None);

        Assert.True(await cache.ExistsAsync("v1:ref:categories:list"));
    }

    [Fact]
    public async Task CacheInvalidationBehavior_KeepsCache_WhenNonGenericResultFails()
    {
        var cache = new FakeCacheService();
        await cache.SetAsync("v1:ref:categories:list", Result<string>.Success("cached"));

        var request = new TestNonGenericInvalidatingCommand(["v1:ref:categories:list"], []);

        var behavior = new CacheInvalidationBehavior<TestNonGenericInvalidatingCommand, Result>(
            cache,
            new TestLogger<CacheInvalidationBehavior<TestNonGenericInvalidatingCommand, Result>>());

        await behavior.Handle(
            request,
            _ => Task.FromResult(Result.Failure(["failed"])),
            CancellationToken.None);

        Assert.True(await cache.ExistsAsync("v1:ref:categories:list"));
    }

    [Fact]
    public async Task CacheInvalidationBehavior_ContinuesInvalidating_WhenOneKeyRemovalFails()
    {
        var cache = new FakeCacheService
        {
            KeysThatFailOnRemove = ["v1:ref:categories:list"]
        };

        await cache.SetAsync("v1:ref:categories:list", Result<string>.Success("cached"));
        await cache.SetAsync("v1:ref:departments:list", Result<string>.Success("cached"));

        var request = new TestInvalidatingCommand(
            ["v1:ref:categories:list", "v1:ref:departments:list"],
            []);

        var behavior = new CacheInvalidationBehavior<TestInvalidatingCommand, Result<string>>(
            cache,
            new TestLogger<CacheInvalidationBehavior<TestInvalidatingCommand, Result<string>>>());

        await behavior.Handle(
            request,
            _ => Task.FromResult(Result<string>.Success("ok")),
            CancellationToken.None);

        Assert.True(await cache.ExistsAsync("v1:ref:categories:list"));
        Assert.False(await cache.ExistsAsync("v1:ref:departments:list"));
    }

    [Fact]
    public void ResultResponse_CanRoundTripThroughJson_ForRedisCache()
    {
        var response = Result<IReadOnlyList<string>>.Success(["one", "two"]);

        var serialized = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<Result<IReadOnlyList<string>>>(serialized);

        Assert.NotNull(deserialized);
        Assert.True(deserialized.IsSuccess);
        Assert.Equal(["one", "two"], deserialized.Data);
        Assert.Empty(deserialized.Errors);
    }

    private sealed record TestCacheableQuery(string CacheKey)
        : IRequest<Result<string>>, ICacheableQuery
    {
        public CacheOptions CacheOptions => CacheDurations.ReferenceData;
    }

    private sealed record TestNonGenericCacheableQuery(string CacheKey)
        : IRequest<Result>, ICacheableQuery
    {
        public CacheOptions CacheOptions => CacheDurations.ReferenceData;
    }

    private sealed record TestPlainQuery : IRequest<Result<string>>;

    private sealed record TestInvalidatingCommand(
        IReadOnlyCollection<string> CacheKeysToRemove,
        IReadOnlyCollection<string> CachePrefixesToRemove)
        : IRequest<Result<string>>, ICacheInvalidatingCommand;

    private sealed record TestNonGenericInvalidatingCommand(
        IReadOnlyCollection<string> CacheKeysToRemove,
        IReadOnlyCollection<string> CachePrefixesToRemove)
        : IRequest<Result>, ICacheInvalidatingCommand;

    private sealed class FakeCacheService : ICacheService
    {
        private readonly Dictionary<string, object?> _cache = [];

        public bool ThrowOnGet { get; init; }
        public bool ThrowOnSet { get; init; }
        public IReadOnlyCollection<string> KeysThatFailOnRemove { get; init; } = [];

        public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
        {
            if (ThrowOnGet)
            {
                throw new InvalidOperationException("Cache read failed.");
            }

            return Task.FromResult(_cache.TryGetValue(key, out var value) ? (T?)value : default);
        }

        public Task SetAsync<T>(
            string key,
            T value,
            CacheOptions? options = null,
            CancellationToken cancellationToken = default)
        {
            if (ThrowOnSet)
            {
                throw new InvalidOperationException("Cache write failed.");
            }

            _cache[key] = value;
            return Task.CompletedTask;
        }

        public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_cache.ContainsKey(key));
        }

        public async Task<T> GetOrSetAsync<T>(
            string key,
            Func<Task<T>> factory,
            CacheOptions? options = null,
            CancellationToken cancellationToken = default)
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

        public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            if (KeysThatFailOnRemove.Contains(key))
            {
                throw new InvalidOperationException("Cache remove failed.");
            }

            _cache.Remove(key);
            return Task.CompletedTask;
        }

        public Task RemoveByPrefixAsync(string prefixKey, CancellationToken cancellationToken = default)
        {
            var keysToRemove = _cache.Keys
                .Where(key => key.StartsWith(prefixKey, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var key in keysToRemove)
            {
                _cache.Remove(key);
            }

            return Task.CompletedTask;
        }
    }

    private sealed class TestLogger<T> : ILogger<T>
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return false;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
        }
    }
}
