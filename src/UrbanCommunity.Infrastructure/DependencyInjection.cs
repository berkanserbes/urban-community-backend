using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Infrastructure.Services;

namespace UrbanCommunity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, TokenService>();
        services.AddFileServices();
        services.AddCaching(configuration);

        return services;
    }

    private static IServiceCollection AddFileServices(this IServiceCollection services)
    {
        var fileProvider = Environment.GetEnvironmentVariable("FILE_STORAGE_PROVIDER") ?? "Local";
        if (fileProvider == "Cloudinary")
        {
            services.AddScoped<IFileService, CloudinaryFileService>();
        }
        else
        {
            services.AddScoped<IFileService, LocalFileService>();
        }

        services.AddScoped<IBackgroundFileUploadService, BackgroundFileUploadService>();
        services.AddHostedService<TempFileCleanupService>();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        var useRedisCache = configuration.GetValue<bool>("CacheSettings:UseRedis");
        if (useRedisCache)
        {
            var redisConnectionString = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddSingleton<StackExchange.Redis.IConnectionMultiplexer>(sp => 
                StackExchange.Redis.ConnectionMultiplexer.Connect(redisConnectionString));
                
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "UrbanCommunity_";
            });
            
            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddMemoryCache();
            services.AddSingleton<ICacheService, InMemoryCacheService>();
        }

        return services;
    }
}