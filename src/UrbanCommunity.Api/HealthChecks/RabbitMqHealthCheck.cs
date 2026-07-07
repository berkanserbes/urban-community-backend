using System.Net.Sockets;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace UrbanCommunity.Api.HealthChecks;

public sealed class RabbitMqHealthCheck(
    IConfiguration configuration)
    : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var host = Environment.GetEnvironmentVariable("RABBIT_MQ_HOST") ?? "localhost";
        var port = configuration.GetValue("RABBIT_MQ_PORT", 5672);

        try
        {
            using var tcpClient = new TcpClient();
            await tcpClient.ConnectAsync(host, port, cancellationToken);

            return HealthCheckResult.Healthy("RabbitMQ is reachable.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("RabbitMQ is not reachable.", ex);
        }
    }
}
