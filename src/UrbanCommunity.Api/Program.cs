using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.PostgreSQL.ColumnWriters;
using UrbanCommunity.Api.Extensions;
using UrbanCommunity.Api.Hubs;
using UrbanCommunity.Application;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure;
using UrbanCommunity.Infrastructure.Consumers;
using UrbanCommunity.Infrastructure.Persistence;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Uygulama başlatılıyor...");

    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.ConfigureKestrelLimits();

    var envFile = builder.Environment.IsDevelopment() ? ".env.development" : ".env";
    if (File.Exists(envFile))
        DotNetEnv.Env.Load(envFile);

    ValidateRequiredConfiguration(builder.Environment);
    var dbConnectionString = GetRequiredEnvironmentVariable("DB_CONNECTION_STRING");

    builder.Host.UseSerilog((context, services, config) =>
    {
        config
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .WriteTo.PostgreSQL(
                connectionString: dbConnectionString ?? string.Empty,
                tableName: "Logs",
                columnOptions: CreateSerilogColumnWriters(),
                needAutoCreateTable: true,
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning);
    });

    builder.Services.AddSingleton<AuditInterceptor>();
    builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
    {
        options.UseNpgsql(dbConnectionString, npgsql => npgsql.UseNetTopologySuite());
        options.AddInterceptors(provider.GetRequiredService<AuditInterceptor>());
    });
    builder.Services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

    builder.Services
        .AddIdentity<User, IdentityRole<Guid>>(options =>
        {
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

    builder.Services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = CreateTokenValidationParameters();
            options.Events = CreateJwtBearerEvents();
        });
    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        var allowedOrigins = builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .GetChildren()
            .Select(origin => origin.Value)
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Cast<string>()
            .ToArray();

        if (allowedOrigins.Length == 0)
            throw new InvalidOperationException("At least one CORS allowed origin must be configured.");

        options.AddPolicy("MobilePolicy", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        x.AddConsumer<
            ReportAiReviewRequestedConsumer,
            ReportAiReviewRequestedConsumerDefinition>();
        x.AddConsumer<ReportAiReviewFaultConsumer>();
        x.AddConsumer<ReportCreatedConsumer>();
        x.AddConsumer<ReportStatusChangedConsumer>();
        x.AddConsumer<ReportDepartmentChangedConsumer>();
        x.AddConsumer<FileUploadConsumer>();
        x.AddConsumer<AnnouncementPublishedConsumer>();

        x.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(
                Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? "localhost",
                "/",
                host =>
                {
                    host.Username(Environment.GetEnvironmentVariable("RABBIT_MQ_USER") ?? "guest");
                    host.Password(Environment.GetEnvironmentVariable("RABBIT_MQ_PASS") ?? "guest");
                });

            cfg.ConfigureEndpoints(context);
        });
    });

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);
    builder.Services.AddApiServices();

    var app = builder.Build();

    await MigrateAndSeedDatabaseAsync(app);

    app.UseMiddleware<UrbanCommunity.Api.Middlewares.ExceptionHandlerMiddleware>();

    app.UseResponseCompression();
    app.UseRequestDecompression();
    app.UseStaticFiles();
    app.UseCors("MobilePolicy");

    if (app.Environment.IsDevelopment())
    {
        app.MapApiDocumentation();
    }

    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHub<NotificationHub>("/hubs/notifications");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Uygulama beklenmedik bir hata nedeniyle sonlandı.");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}

static Dictionary<string, ColumnWriterBase> CreateSerilogColumnWriters()
{
    return new Dictionary<string, ColumnWriterBase>
    {
        ["message"] = new RenderedMessageColumnWriter(),
        ["message_template"] = new MessageTemplateColumnWriter(),
        ["level"] = new LevelColumnWriter(renderAsText: true),
        ["timestamp"] = new TimestampColumnWriter(),
        ["exception"] = new ExceptionColumnWriter(),
        ["log_event"] = new LogEventSerializedColumnWriter(),
        ["properties"] = new PropertiesColumnWriter(),
        ["machine_name"] = new SinglePropertyColumnWriter("MachineName"),
        ["thread_id"] = new SinglePropertyColumnWriter("ThreadId")
    };
}

static TokenValidationParameters CreateTokenValidationParameters()
{
    var jwtSecret = GetRequiredEnvironmentVariable("JWT_SECRET");
    var jwtIssuer = GetRequiredEnvironmentVariable("JWT_ISSUER");
    var jwtAudience = GetRequiredEnvironmentVariable("JWT_AUDIENCE");

    return new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ClockSkew = TimeSpan.FromSeconds(15)
    };
}

static JwtBearerEvents CreateJwtBearerEvents()
{
    return new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            if (!TryGetAccessTokenSessionClaims(
                    context.Principal,
                    out var sessionClaims))
            {
                context.Fail("Access token session claims are invalid.");
                return;
            }

            var validator = context.HttpContext.RequestServices
                .GetRequiredService<IAccessTokenSessionValidator>();

            if (!await validator.IsValidAsync(
                    sessionClaims,
                    context.HttpContext.RequestAborted))
            {
                context.Fail("Access token session is no longer valid.");
            }
        },
        OnAuthenticationFailed = context =>
        {
            Log.Warning(
                context.Exception,
                "JWT authentication failed for {Path}.",
                context.Request.Path);

            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            Log.Warning(
                "JWT challenge for {Path}. AuthenticateFailure: {AuthenticateFailure}",
                context.Request.Path,
                context.AuthenticateFailure?.Message);

            return Task.CompletedTask;
        }
    };
}

static bool TryGetAccessTokenSessionClaims(
    ClaimsPrincipal? principal,
    out AccessTokenSessionClaims sessionClaims)
{
    sessionClaims = null!;

    var userIdValue =
        principal?.FindFirstValue(ClaimTypes.NameIdentifier) ??
        principal?.FindFirstValue(JwtRegisteredClaimNames.Sub);
    var roleValue =
        principal?.FindFirstValue(ClaimTypes.Role) ??
        principal?.FindFirstValue("role");

    if (!Guid.TryParse(userIdValue, out var userId) ||
        !Enum.TryParse<UserRole>(roleValue, ignoreCase: true, out var role) ||
        !Enum.IsDefined(role))
    {
        return false;
    }

    var departmentId = TryGetOptionalGuidClaim(
        principal!,
        "DepartmentId",
        out var departmentClaimValid);
    var jurisdictionId = TryGetOptionalGuidClaim(
        principal!,
        "OfficialJurisdictionId",
        out var jurisdictionClaimValid);
    var jurisdictionLevelValue =
        principal!.FindFirstValue("OfficialJurisdictionLevel");

    OfficialJurisdictionLevel? jurisdictionLevel = null;
    var jurisdictionLevelValid =
        string.IsNullOrWhiteSpace(jurisdictionLevelValue) ||
        (Enum.TryParse<OfficialJurisdictionLevel>(
             jurisdictionLevelValue,
             ignoreCase: true,
             out var parsedJurisdictionLevel) &&
         Enum.IsDefined(parsedJurisdictionLevel) &&
         (jurisdictionLevel = parsedJurisdictionLevel).HasValue);

    if (!departmentClaimValid ||
        !jurisdictionClaimValid ||
        !jurisdictionLevelValid)
    {
        return false;
    }

    var officialClaimsAreComplete =
        departmentId.HasValue &&
        jurisdictionLevel.HasValue &&
        jurisdictionId.HasValue;
    var nonOfficialClaimsAreEmpty =
        !departmentId.HasValue &&
        !jurisdictionLevel.HasValue &&
        !jurisdictionId.HasValue;

    if (role == UserRole.Official
            ? !officialClaimsAreComplete
            : !nonOfficialClaimsAreEmpty)
    {
        return false;
    }

    sessionClaims = new AccessTokenSessionClaims(
        userId,
        role,
        departmentId,
        jurisdictionLevel,
        jurisdictionId);

    return true;
}

static Guid? TryGetOptionalGuidClaim(
    ClaimsPrincipal principal,
    string claimType,
    out bool isValid)
{
    var value = principal.FindFirstValue(claimType);
    if (string.IsNullOrWhiteSpace(value))
    {
        isValid = true;
        return null;
    }

    isValid = Guid.TryParse(value, out var parsed);
    return isValid ? parsed : null;
}

static async Task MigrateAndSeedDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;

    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();

        Log.Information("Veritabanı migrationları uygulanıyor...");
        await context.Database.MigrateAsync();

        var loggerFactory = services.GetRequiredService<ILoggerFactory>();
        var env = services.GetRequiredService<IWebHostEnvironment>();
        var userManager = services.GetRequiredService<UserManager<User>>();
        await ApplicationDbContextSeed.SeedAsync(
            context,
            userManager,
            loggerFactory,
            env.ContentRootPath,
            seedDevelopmentAdmin: env.IsDevelopment());

        Log.Information("Migrationlar ve seed işlemleri başarıyla tamamlandı.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
        throw;
    }
}

static void ValidateRequiredConfiguration(IHostEnvironment environment)
{
    var jwtSecret = GetRequiredEnvironmentVariable("JWT_SECRET");
    if (Encoding.UTF8.GetByteCount(jwtSecret) < 32)
        throw new InvalidOperationException("JWT_SECRET must be at least 32 bytes.");

    GetRequiredEnvironmentVariable("DB_CONNECTION_STRING");
    GetRequiredEnvironmentVariable("JWT_ISSUER");
    GetRequiredEnvironmentVariable("JWT_AUDIENCE");
    GetRequiredPositiveIntEnvironmentVariable("JWT_ACCESS_TOKEN_EXPIRY_MINUTES");
    GetRequiredPositiveIntEnvironmentVariable("JWT_REFRESH_TOKEN_EXPIRY_DAYS");
    GetRequiredEnvironmentVariable("AI_SERVICE_BASE_URL");
    GetRequiredEnvironmentVariable("AI_API_KEY");

    if (!environment.IsDevelopment())
    {
        GetRequiredEnvironmentVariable("RABBIT_MQ_HOST");
        GetRequiredEnvironmentVariable("RABBIT_MQ_USER");
        GetRequiredEnvironmentVariable("RABBIT_MQ_PASS");
    }
}

static string GetRequiredEnvironmentVariable(string key)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrWhiteSpace(value)
        ? throw new InvalidOperationException($"{key} environment variable is required.")
        : value;
}

static int GetRequiredPositiveIntEnvironmentVariable(string key)
{
    var value = GetRequiredEnvironmentVariable(key);
    return int.TryParse(value, out var parsed) && parsed > 0
        ? parsed
        : throw new InvalidOperationException($"{key} must be a positive integer.");
}
