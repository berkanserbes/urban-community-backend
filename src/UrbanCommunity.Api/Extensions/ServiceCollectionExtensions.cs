using System.IO.Compression;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using UrbanCommunity.Api.Constants;
using UrbanCommunity.Api.HealthChecks;
using UrbanCommunity.Api.Services;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Api.Extensions;

public static class ServiceCollectionExtensions
{
    private const string OpenApiDocumentName = "v1";
    private const string BearerSecuritySchemeName = "Bearer";
    private const string TooManyRequestsMessage =
        "Çok fazla istek gönderildi. Lütfen bir süre bekleyip tekrar deneyin.";

    private static readonly JsonSerializerOptions RateLimitJsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services
            .AddApiControllers()
            .AddApiDocumentation()
            .AddCompression()
            .AddUploadLimits()
            .AddAuthRateLimiting()
            .AddRealtimeServices()
            .AddReadinessHealthChecks();

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
                .AddPreferredSecuritySchemes(BearerSecuritySchemeName)
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
            options.AddDocumentTransformer(AddBearerSecuritySchemeAsync);
            options.AddOperationTransformer(AddAuthorizationRequirementAsync);
        });

        return services;
    }

    private static Task AddBearerSecuritySchemeAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes[BearerSecuritySchemeName] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "JWT access token. Enter only the token value without the 'Bearer' prefix."
            };

        return Task.CompletedTask;
    }

    private static Task AddAuthorizationRequirementAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        var metadata = context.Description.ActionDescriptor.EndpointMetadata;

        if (metadata.OfType<IAllowAnonymous>().Any() ||
            !metadata.OfType<IAuthorizeData>().Any())
        {
            return Task.CompletedTask;
        }

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                BearerSecuritySchemeName,
                context.Document)] = []
        });

        return Task.CompletedTask;
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

    private static IServiceCollection AddAuthRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, cancellationToken) =>
            {
                var httpContext = context.HttpContext;

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    httpContext.Response.Headers.RetryAfter = Math.Ceiling(retryAfter.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture);
                }

                httpContext.Response.ContentType = "application/json";

                var response = new
                {
                    IsSuccess = false,
                    Errors = new[] { TooManyRequestsMessage },
                    TraceId = httpContext.TraceIdentifier
                };

                await httpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(response, RateLimitJsonOptions),
                    cancellationToken);
            };

            options.AddPolicy(ApiConstants.AuthLoginRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientPartitionKey(httpContext, "auth-login"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy(ApiConstants.AuthRegisterRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientPartitionKey(httpContext, "auth-register"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromMinutes(10),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));

            options.AddPolicy(ApiConstants.AuthRefreshTokenRateLimitPolicy, httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientPartitionKey(httpContext, "auth-refresh"),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(5),
                        QueueLimit = 0,
                        AutoReplenishment = true
                    }));
        });

        return services;
    }

    private static string GetClientPartitionKey(HttpContext context, string endpointKey)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString();
        return string.IsNullOrWhiteSpace(clientIp)
            ? $"{endpointKey}:unknown"
            : $"{endpointKey}:{clientIp}";
    }

    private static IServiceCollection AddRealtimeServices(this IServiceCollection services)
    {
        services.AddSignalR();
        services.AddScoped<IRealtimeNotificationService, SignalRNotificationService>();

        return services;
    }

    private static IServiceCollection AddReadinessHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("postgresql", tags: ["ready"])
            .AddCheck<RedisHealthCheck>("redis", tags: ["ready"])
            .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"])
            .AddCheck<AiServiceHealthCheck>("ai-service", tags: ["ready"]);

        return services;
    }
}
