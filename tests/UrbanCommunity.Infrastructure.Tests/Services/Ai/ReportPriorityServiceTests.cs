using Microsoft.Extensions.Logging.Abstractions;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Services.Ai;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services.Ai;

public sealed class ReportPriorityServiceTests
{
    [Theory]
    [InlineData(0.00, ReportPriority.Low)]
    [InlineData(0.25, ReportPriority.Low)]
    [InlineData(0.26, ReportPriority.Medium)]
    [InlineData(0.50, ReportPriority.Medium)]
    [InlineData(0.51, ReportPriority.High)]
    [InlineData(0.75, ReportPriority.High)]
    [InlineData(0.76, ReportPriority.Urgent)]
    [InlineData(1.00, ReportPriority.Urgent)]
    public void GetPriorityForScore_UsesExpectedBoundaries(
        double score,
        ReportPriority expected)
    {
        var result = ReportPriorityService.GetPriorityForScore(score);

        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task SuggestPriorityAsync_ReturnsMappedSuggestion_WhenResponseIsValid()
    {
        var client = new StubAiApiClient
        {
            PriorityResponse = new AiPriorityResponse(
                0.82,
                "Urgent",
                "Acil müdahale gerekli.")
        };
        var service = CreateService(client);

        var result = await service.SuggestPriorityAsync(
            "Su kaçağı",
            "Ana caddede yoğun su kaçağı bulunuyor.",
            "Su ve Kanalizasyon",
            0,
            0);

        Assert.NotNull(result);
        Assert.Equal(ReportPriority.Urgent, result.Priority);
        Assert.Equal(0.82, result.Score);
    }

    [Theory]
    [InlineData(-0.01, "Low")]
    [InlineData(1.01, "Urgent")]
    [InlineData(0.90, "High")]
    [InlineData(0.40, "Unknown")]
    public async Task SuggestPriorityAsync_ReturnsNull_WhenResponseIsInvalid(
        double score,
        string priority)
    {
        var client = new StubAiApiClient
        {
            PriorityResponse = new AiPriorityResponse(
                score,
                priority,
                "Geçersiz cevap")
        };
        var service = CreateService(client);

        var result = await service.SuggestPriorityAsync(
            "Başlık",
            "Yeterince uzun rapor açıklaması.",
            "Kategori",
            0,
            0);

        Assert.Null(result);
    }

    [Fact]
    public async Task SuggestPriorityAsync_ReturnsNull_WhenAiServiceIsUnavailable()
    {
        var service = CreateService(new StubAiApiClient());

        var result = await service.SuggestPriorityAsync(
            "Başlık",
            "Yeterince uzun rapor açıklaması.",
            "Kategori",
            0,
            0);

        Assert.Null(result);
    }

    [Fact]
    public async Task SuggestPriorityAsync_ReturnsNull_WhenReasoningIsTooLong()
    {
        var client = new StubAiApiClient
        {
            PriorityResponse = new AiPriorityResponse(
                0.82,
                "Urgent",
                new string('x', 501))
        };
        var service = CreateService(client);

        var result = await service.SuggestPriorityAsync(
            "Baslik",
            "Yeterince uzun rapor aciklamasi.",
            "Kategori",
            0,
            0);

        Assert.Null(result);
    }

    private static ReportPriorityService CreateService(IAiApiClient client)
    {
        return new ReportPriorityService(
            client,
            NullLogger<ReportPriorityService>.Instance);
    }

    private sealed class StubAiApiClient : IAiApiClient
    {
        public AiPriorityResponse? PriorityResponse { get; init; }

        public Task<AiCategorizationResponse?> SuggestReportCategoryAsync(
            AiCategorizationRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AiCategorizationResponse?>(null);
        }

        public Task<AiPriorityResponse?> SuggestReportPriorityAsync(
            AiPriorityRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(PriorityResponse);
        }

        public Task<AiSimilarityResponse?> FindSimilarReportsAsync(
            AiSimilarityRequest request,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AiSimilarityResponse?>(null);
        }
    }
}
