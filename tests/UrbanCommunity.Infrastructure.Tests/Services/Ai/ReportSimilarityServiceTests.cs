using Microsoft.Extensions.Logging.Abstractions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Infrastructure.Services.Ai;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services.Ai;

public sealed class ReportSimilarityServiceTests
{
    [Fact]
    public async Task FindSimilarReportsAsync_MapsAndSortsValidMatches()
    {
        var firstId = Guid.NewGuid();
        var secondId = Guid.NewGuid();
        var client = new StubAiApiClient
        {
            SimilarityResponse = new AiSimilarityResponse(
            [
                new AiSimilarityMatch(secondId.ToString(), 0.60, "Dusuk benzerlik."),
                new AiSimilarityMatch(firstId.ToString(), 0.92, "Ayni olay.")
            ])
        };
        var service = CreateService(client);

        var result = await service.FindSimilarReportsAsync(
            "Su borusu patladi",
            "Caddede patlayan borudan yola su akiyor.",
            [
                new(firstId, "Su arizasi", "Caddede boru patladi ve su akiyor.", DateTime.UtcNow),
                new(secondId, "Yol sorunu", "Yolda genel bir altyapi sorunu var.", DateTime.UtcNow)
            ]);

        Assert.NotNull(result);
        Assert.Equal(firstId, result[0].ReportId);
        Assert.Equal(0.92, result[0].SimilarityScore);
    }

    [Fact]
    public async Task FindSimilarReportsAsync_ReturnsNullForUnknownCandidate()
    {
        var client = new StubAiApiClient
        {
            SimilarityResponse = new AiSimilarityResponse(
            [
                new AiSimilarityMatch(Guid.NewGuid().ToString(), 0.90, "Gecersiz aday.")
            ])
        };
        var service = CreateService(client);

        var result = await service.FindSimilarReportsAsync(
            "Su borusu patladi",
            "Caddede patlayan borudan yola su akiyor.",
            [
                new(Guid.NewGuid(), "Su arizasi", "Caddede boru patladi.", DateTime.UtcNow)
            ]);

        Assert.Null(result);
    }

    [Fact]
    public async Task FindSimilarReportsAsync_ReturnsNullForMissingMatchCollection()
    {
        var client = new StubAiApiClient
        {
            SimilarityResponse = new AiSimilarityResponse(null!)
        };
        var service = CreateService(client);

        var result = await service.FindSimilarReportsAsync(
            "Su borusu patladi",
            "Caddede patlayan borudan yola su akiyor.",
            [
                new(
                    Guid.NewGuid(),
                    "Su arizasi",
                    "Caddede boru patladi.",
                    DateTime.UtcNow)
            ]);

        Assert.Null(result);
    }

    private static ReportSimilarityService CreateService(IAiApiClient client) =>
        new(client, NullLogger<ReportSimilarityService>.Instance);

    private sealed class StubAiApiClient : IAiApiClient
    {
        public AiSimilarityResponse? SimilarityResponse { get; init; }

        public Task<AiCategorizationResponse?> SuggestReportCategoryAsync(
            AiCategorizationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AiCategorizationResponse?>(null);

        public Task<AiPriorityResponse?> SuggestReportPriorityAsync(
            AiPriorityRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AiPriorityResponse?>(null);

        public Task<AiSimilarityResponse?> FindSimilarReportsAsync(
            AiSimilarityRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(SimilarityResponse);
    }
}
