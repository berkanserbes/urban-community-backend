using System.IO.Compression;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using UrbanCommunity.Api.Constants;
using UrbanCommunity.Api.Services;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private const string OpenApiDocumentName = "v1";

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services
            .AddApiControllers()
            .AddApiDocumentation()
            .AddCompression()
            .AddUploadLimits()
            .AddRealtimeServices();

        return services;
    }

    public static IWebHostBuilder ConfigureKestrelLimits(this IWebHostBuilder webHost)
    {
        return webHost.ConfigureKestrel(options =>
        {
            options.Limits.MaxRequestBodySize = ApiConstants.MaxUploadSize;
        });
    }

    public static IEndpointRouteBuilder MapApiDocumentation(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapOpenApi();

        endpoints.MapScalarApiReference(options =>
        {
            options
                .WithTitle("UrbanCommunity API")
                .WithTheme(ScalarTheme.Purple)
                .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return endpoints;
    }

    private static IServiceCollection AddApiControllers(this IServiceCollection services)
    {
        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        return services;
    }

    private static IServiceCollection AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        services.AddOpenApi(OpenApiDocumentName, options =>
        {
            options.AddSchemaTransformer(NormalizeGuidSchemaAsync);
        });

        return services;
    }

    private static Task NormalizeGuidSchemaAsync(
        OpenApiSchema schema,
        OpenApiSchemaTransformerContext context,
        CancellationToken cancellationToken)
    {
        var type = context.JsonTypeInfo.Type;

        if (type == typeof(Guid) || type == typeof(Guid?))
        {
            schema.Type = JsonSchemaType.String;
            schema.Format = "uuid";
        }

        return Task.CompletedTask;
    }

    private static IServiceCollection AddCompression(this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            [
                "application/json",
                "text/plain",
                "text/html"
            ]);
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        services.AddRequestDecompression();

        return services;
    }

    private static IServiceCollection AddUploadLimits(this IServiceCollection services)
    {
        services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(options =>
        {
            options.MultipartBodyLengthLimit = ApiConstants.MaxUploadSize;
        });

        return services;
    }

    private static IServiceCollection AddRealtimeServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        return services;
    }
}
