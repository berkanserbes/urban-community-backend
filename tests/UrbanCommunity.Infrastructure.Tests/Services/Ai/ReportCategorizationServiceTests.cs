using Microsoft.Extensions.Logging.Abstractions;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Infrastructure.Services.Ai;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services.Ai;

public sealed class ReportCategorizationServiceTests
{
    [Fact]
    public async Task SuggestCategoryAsync_MapsValidResponse()
    {
        var categoryId = Guid.NewGuid();
        var service = CreateService(new StubAiApiClient
        {
            CategorizationResponse = new AiCategorizationResponse(
                categoryId.ToString(),
                " Altyapi ",
                0.91,
                " Yol hasari. ")
        });

        var result = await service.SuggestCategoryAsync(
            "Yol coktu",
            "Mahalle girisindeki yol yuzeyi coktu.",
            [new(categoryId, "Altyapi", null)]);

        Assert.NotNull(result);
        Assert.Equal(categoryId, result.CategoryId);
        Assert.Equal("Altyapi", result.CategoryName);
        Assert.Equal("Yol hasari.", result.Reasoning);
    }

    [Theory]
    [InlineData("", "Valid reasoning")]
    [InlineData("Category", "")]
    public async Task SuggestCategoryAsync_ReturnsNullForMissingText(
        string categoryName,
        string reasoning)
    {
        var categoryId = Guid.NewGuid();
        var service = CreateService(new StubAiApiClient
        {
            CategorizationResponse = new AiCategorizationResponse(
                categoryId.ToString(),
                categoryName,
                0.91,
                reasoning)
        });

        var result = await service.SuggestCategoryAsync(
            "Yol coktu",
            "Mahalle girisindeki yol yuzeyi coktu.",
            [new(categoryId, "Altyapi", null)]);

        Assert.Null(result);
    }

    private static ReportCategorizationService CreateService(IAiApiClient client) =>
        new(client, NullLogger<ReportCategorizationService>.Instance);

    private sealed class StubAiApiClient : IAiApiClient
    {
        public AiCategorizationResponse? CategorizationResponse { get; init; }

        public Task<AiCategorizationResponse?> SuggestReportCategoryAsync(
            AiCategorizationRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(CategorizationResponse);

        public Task<AiPriorityResponse?> SuggestReportPriorityAsync(
            AiPriorityRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AiPriorityResponse?>(null);

        public Task<AiSimilarityResponse?> FindSimilarReportsAsync(
            AiSimilarityRequest request,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<AiSimilarityResponse?>(null);
    }
}
