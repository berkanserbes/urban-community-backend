using System.IO.Compression;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.ResponseCompression;
using UrbanCommunity.Api.Constants;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Api.Services;

namespace UrbanCommunity.Api.Extensions;

/// <summary>
/// Extension methods for configuring API layer services.
/// Provides convenient methods to register SignalR, compression, and other API-specific services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds API layer services to the dependency injection container.
    /// Includes SignalR, compression, upload limits, controllers, and API documentation.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        // =============================================
        // SIGNALR
        // =============================================
        services.AddSignalR();

        // =============================================
        // RESPONSE & REQUEST COMPRESSION
        // =============================================
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes
                .Concat(["application/json", "text/plain", "text/html"]);
        });

        services.Configure<BrotliCompressionProviderOptions>(o => o.Level = CompressionLevel.Fastest);
        services.Configure<GzipCompressionProviderOptions>(o => o.Level = CompressionLevel.Optimal);

        services.AddRequestDecompression();

        // =============================================
        // UPLOAD LIMITS (supports large video files)
        // =============================================
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = ApiConstants.MaxUploadSize;
        });

        // =============================================
        // REALTIME NOTIFICATION SERVICE
        // =============================================
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        // =============================================
        // CONTROLLERS & API DOCUMENTATION
        // =============================================
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        services.AddEndpointsApiExplorer();
        // TODO: .NET 10 OpenAPI feature - Guid serialization issue in JsonSchemaExporter
        // services.AddOpenApi();

        return services;
    }

    /// <summary>
    /// Configures Kestrel server limits for handling large file uploads.
    /// Must be called on the WebHost configuration.
    /// </summary>
    public static IWebHostBuilder ConfigureKestrelLimits(this IWebHostBuilder webHost)
    {
        return webHost.ConfigureKestrel(serverOptions =>
        {
            serverOptions.Limits.MaxRequestBodySize = ApiConstants.MaxUploadSize;
        });
    }
}
