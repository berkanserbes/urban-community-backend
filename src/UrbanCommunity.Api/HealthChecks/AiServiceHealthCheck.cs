using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UrbanCommunity.Api.HealthChecks;

public sealed class AiServiceHealthCheck(
    IHttpClientFactory httpClientFactory)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var baseUrl = Environment.GetEnvironmentVariable("AI_SERVICE_BASE_URL");
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return HealthCheckResult.Unhealthy("AI_SERVICE_BASE_URL is not configured.");
        }

        var apiKey = Environment.GetEnvironmentVariable("AI_API_KEY");
        var client = httpClientFactory.CreateClient();
        client.Timeout = TimeSpan.FromSeconds(5);

        var readyUri = new Uri(new Uri(baseUrl.EndsWith('/') ? baseUrl : $"{baseUrl}/"), "health/ready");
        using var request = new HttpRequestMessage(HttpMethod.Get, readyUri);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            request.Headers.Add("X-Api-Key", apiKey);
        }

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy("AI service is ready.")
                : HealthCheckResult.Unhealthy($"AI service readiness returned HTTP {(int)response.StatusCode}.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("AI service readiness check failed.", ex);
        }
    }
}
