using Aspire.Hosting;
using Microsoft.Extensions.Configuration;

LoadEnvironmentFile("backend", "src", "UrbanCommunity.Api", ".env.development");
LoadEnvironmentFile("ai", ".env.development");

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("env");

var postgresUser = builder.AddParameter(
    "postgres-user",
    () => builder.Configuration["POSTGRES_USER"] ?? "postgres");
var postgresPassword = builder.AddParameter(
    "postgres-password",
    () => builder.Configuration["POSTGRES_PASSWORD"] ?? throw new InvalidOperationException("POSTGRES_PASSWORD is required."),
    secret: true);
var postgresDatabaseName =
    builder.Configuration["POSTGRES_DB"] ??
    "UrbanCommunityDb";
var postgresPort =
    builder.Configuration.GetValue<int?>("POSTGRES_PORT") ??
    5432;
var postgresDataVolumeName =
    builder.Configuration["POSTGRES_DATA_VOLUME_NAME"];
    
var postgresImage = builder.Configuration["Postgres:Image"] ?? "postgis/postgis";
var postgresTag = builder.Configuration["Postgres:Tag"] ?? "latest";

var rabbitUserValue = builder.Configuration["RABBIT_MQ_USER"] ?? "guest";
var rabbitUser = builder.AddParameter("rabbitmq-user", () => rabbitUserValue);
var rabbitPassword = builder.AddParameter(
    "rabbitmq-password",
    () => builder.Configuration["RABBIT_MQ_PASS"] ?? "guest",
    secret: true);
var rabbitPort = builder.Configuration.GetValue("RabbitMQ:Port", 5672);
var rabbitManagementPort = builder.Configuration.GetValue("RabbitMQ:ManagementPort", 15672);
var rabbitImage = builder.Configuration["RabbitMQ:Image"] ?? "rabbitmq";
var rabbitTag = builder.Configuration["RabbitMQ:Tag"] ?? "3-management-alpine";

var redisPort =
    builder.Configuration.GetValue<int?>("REDIS_PORT") ??
    6379;
var redisImage = builder.Configuration["Redis:Image"] ?? "redis";
var redisTag = builder.Configuration["Redis:Tag"] ?? "7-alpine";

var aiImage = builder.Configuration["AiService:Image"] ?? "urban-community-ai";
var aiTag = builder.Configuration["AiService:Tag"] ?? "latest";
var aiPort = builder.Configuration.GetValue("AiService:Port", 8000);
var aiApiKey = builder.AddParameter(
    "ai-api-key",
    () => builder.Configuration["AI_API_KEY"] ?? throw new InvalidOperationException("AI_API_KEY is required."),
    secret: true);
var openAiApiKey = builder.AddParameter(
    "openai-api-key",
    () => builder.Configuration["OPENAI_API_KEY"] ?? string.Empty,
    secret: true);
var geminiApiKey = builder.AddParameter(
    "gemini-api-key",
    () => builder.Configuration["GEMINI_API_KEY"] ?? string.Empty,
    secret: true);
var deepSeekApiKey = builder.AddParameter(
    "deepseek-api-key",
    () => builder.Configuration["DEEPSEEK_API_KEY"] ?? string.Empty,
    secret: true);

var jwtSecret = builder.AddParameter(
    "jwt-secret",
    () => builder.Configuration["JWT_SECRET"] ?? throw new InvalidOperationException("JWT_SECRET is required."),
    secret: true);
var jwtIssuer = builder.Configuration["JWT_ISSUER"] ?? "UrbanCommunity";
var jwtAudience = builder.Configuration["JWT_AUDIENCE"] ?? "UrbanCommunity";
var jwtAccessTokenExpiryMinutes =
    builder.Configuration["JWT_ACCESS_TOKEN_EXPIRY_MINUTES"] ??
    "60";
var jwtRefreshTokenExpiryDays =
    builder.Configuration["JWT_REFRESH_TOKEN_EXPIRY_DAYS"] ??
    "7";
var fileStorageProvider = builder.Configuration["FILE_STORAGE_PROVIDER"] ?? "Local";
var cloudinaryCloudName = builder.AddParameter(
    "cloudinary-cloud-name",
    () => builder.Configuration["CLOUDINARY_CLOUD_NAME"] ?? string.Empty,
    secret: true);
var cloudinaryApiKey = builder.AddParameter(
    "cloudinary-api-key",
    () => builder.Configuration["CLOUDINARY_API_KEY"] ?? string.Empty,
    secret: true);
var cloudinaryApiSecret = builder.AddParameter(
    "cloudinary-api-secret",
    () => builder.Configuration["CLOUDINARY_API_SECRET"] ?? string.Empty,
    secret: true);

var postgres = builder.AddPostgres(
        "postgres",
        userName: postgresUser,
        password: postgresPassword,
        port: postgresPort)
    .WithImage(postgresImage)
    .WithImageTag(postgresTag);

postgres = string.IsNullOrWhiteSpace(postgresDataVolumeName)
    ? postgres.WithDataVolume()
    : postgres.WithDataVolume(postgresDataVolumeName);

var postgresDatabase = postgres.AddDatabase("urban-community-db", postgresDatabaseName);

var redis = builder.AddRedis("redis", port: redisPort)
    .WithImage(redisImage)
    .WithImageTag(redisTag);

var rabbitmq = builder.AddRabbitMQ(
        "rabbitmq",
        userName: rabbitUser,
        password: rabbitPassword,
        port: rabbitPort)
    .WithImage(rabbitImage)
    .WithImageTag(rabbitTag)
    .WithHttpEndpoint(port: rabbitManagementPort, targetPort: 15672, name: "management");

var aiService = builder.AddContainer("ai-service", aiImage, aiTag)
    .WithHttpEndpoint(port: aiPort, targetPort: 8000, name: "http")
    .WithHttpHealthCheck("/health/ready")
    .WithEnvironment("AI_API_KEY", aiApiKey)
    .WithEnvironment("LLM_PROVIDER", builder.Configuration["LLM_PROVIDER"] ?? "openai")
    .WithEnvironment("OPENAI_API_KEY", openAiApiKey)
    .WithEnvironment("GEMINI_API_KEY", geminiApiKey)
    .WithEnvironment("DEEPSEEK_API_KEY", deepSeekApiKey)
    .WithEnvironment("DEEPSEEK_MODEL", builder.Configuration["DEEPSEEK_MODEL"] ?? "deepseek-v4-flash")
    .WithEnvironment("DEEPSEEK_BASE_URL", builder.Configuration["DEEPSEEK_BASE_URL"] ?? "https://api.deepseek.com");

builder.AddProject<Projects.UrbanCommunity_Api>("api")
    .WithReference(postgresDatabase)
    .WithReference(redis)
    .WithReference(rabbitmq)
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health/live")
    .WaitFor(postgresDatabase)
    .WaitFor(redis)
    .WaitFor(rabbitmq)
    .WaitFor(aiService)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("DB_CONNECTION_STRING", postgresDatabase)
    .WithEnvironment("JWT_SECRET", jwtSecret)
    .WithEnvironment("JWT_ISSUER", jwtIssuer)
    .WithEnvironment("JWT_AUDIENCE", jwtAudience)
    .WithEnvironment("JWT_ACCESS_TOKEN_EXPIRY_MINUTES", jwtAccessTokenExpiryMinutes)
    .WithEnvironment("JWT_REFRESH_TOKEN_EXPIRY_DAYS", jwtRefreshTokenExpiryDays)
    .WithEnvironment("RABBIT_MQ_HOST", "localhost")
    .WithEnvironment("RABBIT_MQ_USER", rabbitUser)
    .WithEnvironment("RABBIT_MQ_PASS", rabbitPassword)
    .WithEnvironment("ConnectionStrings__Redis", redis)
    .WithEnvironment("CacheSettings__UseRedis", "true")
    .WithEnvironment("CacheSettings__KeyPrefix", "urban-community")
    .WithEnvironment("AI_SERVICE_BASE_URL", aiService.GetEndpoint("http"))
    .WithEnvironment("AI_API_KEY", aiApiKey)
    .WithEnvironment("FILE_STORAGE_PROVIDER", fileStorageProvider)
    .WithEnvironment("CLOUDINARY_CLOUD_NAME", cloudinaryCloudName)
    .WithEnvironment("CLOUDINARY_API_KEY", cloudinaryApiKey)
    .WithEnvironment("CLOUDINARY_API_SECRET", cloudinaryApiSecret);

builder.Build().Run();

static void LoadEnvironmentFile(params string[] relativePathParts)
{
    var envPath = FindFileFromCurrentDirectory(relativePathParts);
    if (envPath is null)
    {
        return;
    }

    foreach (var rawLine in File.ReadAllLines(envPath))
    {
        var line = rawLine.Trim();
        if (line.Length == 0 || line.StartsWith('#'))
        {
            continue;
        }

        var separatorIndex = line.IndexOf('=');
        if (separatorIndex <= 0)
        {
            continue;
        }

        var key = line[..separatorIndex].Trim();
        var value = line[(separatorIndex + 1)..].Trim().Trim('"');

        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(key)))
        {
            Environment.SetEnvironmentVariable(key, value);
        }
    }
}

static string? FindFileFromCurrentDirectory(params string[] relativePathParts)
{
    var currentDirectory = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (currentDirectory is not null)
    {
        var candidate = Path.Combine(
            [currentDirectory.FullName, .. relativePathParts]);

        if (File.Exists(candidate))
        {
            return candidate;
        }

        currentDirectory = currentDirectory.Parent;
    }

    return null;
}
