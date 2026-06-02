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
    DotNetEnv.Env.Load(envFile);

    var dbConnectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING");

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

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = CreateTokenValidationParameters();
            options.Events = CreateJwtBearerEvents();
        });
    builder.Services.AddAuthorization();

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("MobilePolicy", policy =>
        {
            policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    });

    builder.Services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();

        x.AddConsumer<ReportCreatedConsumer>();
        x.AddConsumer<ReportStatusChangedConsumer>();
        x.AddConsumer<DepartmentAssignedConsumer>();
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
    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "super-secret-key-that-should-be-changed";
    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "UrbanCommunity";
    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? "UrbanCommunityApp";

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
        }
    };
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
        await ApplicationDbContextSeed.SeedAsync(context, loggerFactory, env.ContentRootPath);

        Log.Information("Migrationlar ve seed işlemleri başarıyla tamamlandı.");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred during database migration or seeding.");
    }
}
