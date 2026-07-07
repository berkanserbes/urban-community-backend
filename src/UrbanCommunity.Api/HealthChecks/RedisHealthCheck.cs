using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace UrbanCommunity.Api.HealthChecks;

public sealed class RedisHealthCheck(
    IConfiguration configuration,
    IServiceProvider serviceProvider)
    : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var redisEnabled = configuration.GetValue<bool>("CacheSettings:UseRedis");
        if (!redisEnabled)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Redis cache is disabled."));
        }

        var connectionMultiplexer = serviceProvider.GetService<IConnectionMultiplexer>();
        if (connectionMultiplexer is null)
        {
            return Task.FromResult(HealthCheckResult.Degraded("Redis is enabled but the app is using cache fallback."));
        }

        return Task.FromResult(connectionMultiplexer.IsConnected
            ? HealthCheckResult.Healthy("Redis is connected.")
            : HealthCheckResult.Unhealthy("Redis is not connected."));
    }
}
