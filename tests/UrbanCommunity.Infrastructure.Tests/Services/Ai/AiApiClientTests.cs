using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using UrbanCommunity.Infrastructure.Services.Ai;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services.Ai;

public sealed class AiApiClientTests
{
    [Theory]
    [InlineData(HttpStatusCode.RequestTimeout)]
    [InlineData(HttpStatusCode.TooManyRequests)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task SuggestReportPriorityAsync_ThrowsForTransientStatusCodes(
        HttpStatusCode statusCode)
    {
        var client = CreateClient(new StubHandler(
            new HttpResponseMessage(statusCode)));

        await Assert.ThrowsAsync<AiServiceTransientException>(() =>
            client.SuggestReportPriorityAsync(CreatePriorityRequest()));
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.Unauthorized)]
    [InlineData(HttpStatusCode.UnprocessableEntity)]
    [InlineData(HttpStatusCode.FailedDependency)]
    public async Task SuggestReportPriorityAsync_ReturnsNullForPermanentStatusCodes(
        HttpStatusCode statusCode)
    {
        var client = CreateClient(new StubHandler(
            new HttpResponseMessage(statusCode)));

        var result = await client.SuggestReportPriorityAsync(
            CreatePriorityRequest());

        Assert.Null(result);
    }

    [Fact]
    public async Task SuggestReportPriorityAsync_ReturnsNullForMalformedJson()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                "{invalid-json",
                Encoding.UTF8,
                "application/json")
        };
        var client = CreateClient(new StubHandler(response));

        var result = await client.SuggestReportPriorityAsync(
            CreatePriorityRequest());

        Assert.Null(result);
    }

    [Fact]
    public async Task SuggestReportPriorityAsync_ThrowsWhenTransportFails()
    {
        var client = CreateClient(new ThrowingHandler());

        await Assert.ThrowsAsync<AiServiceTransientException>(() =>
            client.SuggestReportPriorityAsync(CreatePriorityRequest()));
    }

    [Fact]
    public async Task SuggestReportPriorityAsync_PropagatesActivityTraceId()
    {
        var response = CreateJsonResponse(
            """
            {
              "priority_score": 0.82,
              "suggested_priority": "Urgent",
              "reasoning": "Acil mudahale gerekli."
            }
            """);
        var handler = new CapturingHandler(response);
        var client = CreateClient(handler);
        using var activity = new Activity("ai-test").Start();

        await client.SuggestReportPriorityAsync(CreatePriorityRequest());

        Assert.NotNull(handler.Request);
        Assert.Equal(
            activity.TraceId.ToString(),
            handler.Request.Headers.GetValues("X-Correlation-ID").Single());
    }

    [Fact]
    public async Task SuggestReportPriorityAsync_UsesSnakeCaseContract()
    {
        var response = CreateJsonResponse(
            """
            {
              "priority_score": 0.82,
              "suggested_priority": "Urgent",
              "reasoning": "Acil mudahale gerekli."
            }
            """);
        var handler = new CapturingHandler(response);
        var client = CreateClient(handler);

        var result = await client.SuggestReportPriorityAsync(
            CreatePriorityRequest());

        Assert.NotNull(result);
        Assert.Equal(0.82, result.PriorityScore);
        Assert.Equal("Urgent", result.SuggestedPriority);

        using var document = JsonDocument.Parse(handler.RequestBody!);
        var root = document.RootElement;
        Assert.Equal("Kategori", root.GetProperty("category_name").GetString());
        Assert.Equal(0, root.GetProperty("similar_report_count").GetInt32());
        Assert.Equal(0, root.GetProperty("days_open").GetInt32());
        Assert.False(root.TryGetProperty("categoryName", out _));
    }

    [Fact]
    public async Task SuggestReportCategoryAsync_ReadsSnakeCaseResponse()
    {
        var categoryId = Guid.NewGuid();
        var response = CreateJsonResponse(
            $$"""
            {
              "suggested_category_id": "{{categoryId}}",
              "suggested_category_name": "Altyapi",
              "confidence": 0.91,
              "reasoning": "Altyapi sorunu."
            }
            """);
        var client = CreateClient(new StubHandler(response));

        var result = await client.SuggestReportCategoryAsync(
            new AiCategorizationRequest(
                "Yol coktu",
                "Mahalle girisindeki yol yuzeyi coktu.",
                [new AiCategoryItem(categoryId.ToString(), "Altyapi", null)]));

        Assert.NotNull(result);
        Assert.Equal(categoryId.ToString(), result.SuggestedCategoryId);
        Assert.Equal("Altyapi", result.SuggestedCategoryName);
    }

    [Fact]
    public async Task FindSimilarReportsAsync_UsesSnakeCaseContract()
    {
        var reportId = Guid.NewGuid();
        var response = CreateJsonResponse(
            $$"""
            {
              "similar_reports": [
                {
                  "report_id": "{{reportId}}",
                  "similarity_score": 0.88,
                  "reasoning": "Ayni fiziksel sorun."
                }
              ]
            }
            """);
        var handler = new CapturingHandler(response);
        var client = CreateClient(handler);

        var result = await client.FindSimilarReportsAsync(
            new AiSimilarityRequest(
                "Su kacagi",
                "Market onunde yola su akiyor.",
                [
                    new AiSimilarityCandidate(
                        reportId.ToString(),
                        "Borudan su akiyor",
                        "Market onundeki borudan yola su akiyor.",
                        DateTime.UtcNow)
                ]));

        Assert.NotNull(result);
        Assert.Single(result.SimilarReports);
        Assert.Equal(reportId.ToString(), result.SimilarReports[0].ReportId);

        using var document = JsonDocument.Parse(handler.RequestBody!);
        var root = document.RootElement;
        var candidate = root.GetProperty("existing_reports")[0];
        Assert.True(candidate.TryGetProperty("created_at", out _));
        Assert.False(root.TryGetProperty("existingReports", out _));
    }

    private static HttpResponseMessage CreateJsonResponse(string json)
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(json, Encoding.UTF8, "application/json")
        };
    }

    private static AiApiClient CreateClient(HttpMessageHandler handler)
    {
        return new AiApiClient(
            new HttpClient(handler)
            {
                BaseAddress = new Uri("http://ai-service/")
            },
            NullLogger<AiApiClient>.Instance);
    }

    private static AiPriorityRequest CreatePriorityRequest()
    {
        return new AiPriorityRequest(
            "Baslik",
            "Yeterince uzun rapor aciklamasi.",
            "Kategori",
            0,
            0);
    }

    private sealed class StubHandler(HttpResponseMessage response)
        : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(response);
        }
    }

    private sealed class CapturingHandler(HttpResponseMessage response)
        : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? RequestBody { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestBody = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);
            return response;
        }
    }

    private sealed class ThrowingHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            throw new HttpRequestException("Connection failed.");
        }
    }
}
