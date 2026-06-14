using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace UrbanCommunity.Infrastructure.Services.Ai;

internal interface IAiApiClient
{
    Task<AiCategorizationResponse?> SuggestReportCategoryAsync(
        AiCategorizationRequest request,
        CancellationToken cancellationToken = default);

    Task<AiPriorityResponse?> SuggestReportPriorityAsync(
        AiPriorityRequest request,
        CancellationToken cancellationToken = default);

    Task<AiSimilarityResponse?> FindSimilarReportsAsync(
        AiSimilarityRequest request,
        CancellationToken cancellationToken = default);
}

internal sealed class AiApiClient(
    HttpClient httpClient,
    ILogger<AiApiClient> logger) : IAiApiClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public async Task<AiCategorizationResponse?> SuggestReportCategoryAsync(
        AiCategorizationRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<AiCategorizationRequest, AiCategorizationResponse>(
            "api/categorization/suggest",
            request,
            cancellationToken);
    }

    public async Task<AiPriorityResponse?> SuggestReportPriorityAsync(
        AiPriorityRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<AiPriorityRequest, AiPriorityResponse>(
            "api/priority/score",
            request,
            cancellationToken);
    }

    public async Task<AiSimilarityResponse?> FindSimilarReportsAsync(
        AiSimilarityRequest request,
        CancellationToken cancellationToken = default)
    {
        return await PostAsync<AiSimilarityRequest, AiSimilarityResponse>(
            "api/similarity/find",
            request,
            cancellationToken);
    }

    private async Task<TResponse?> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
        where TResponse : class
    {
        try
        {
            using var message = new HttpRequestMessage(
                HttpMethod.Post,
                endpoint)
            {
                Content = JsonContent.Create(
                    request,
                    options: SerializerOptions)
            };
            message.Headers.TryAddWithoutValidation(
                "X-Correlation-ID",
                Activity.Current?.TraceId.ToString() ?? Guid.NewGuid().ToString());

            using var response = await httpClient.SendAsync(
                message,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "AI request to {Endpoint} failed with status code {StatusCode}.",
                    endpoint,
                    response.StatusCode);

                if (IsTransient(response.StatusCode))
                {
                    throw new AiServiceTransientException(
                        $"AI request to {endpoint} failed temporarily with status code {(int)response.StatusCode}.");
                }

                return null;
            }

            return await response.Content.ReadFromJsonAsync<TResponse>(
                SerializerOptions,
                cancellationToken);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning("AI request to {Endpoint} timed out.", endpoint);
            throw new AiServiceTransientException(
                $"AI request to {endpoint} timed out.");
        }
        catch (HttpRequestException exception)
        {
            logger.LogWarning(exception, "AI service could not be reached.");
            throw new AiServiceTransientException(
                "AI service could not be reached.",
                exception);
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "AI service returned malformed JSON.");
            return null;
        }
        catch (AiServiceTransientException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unexpected AI API request failure.");
            return null;
        }
    }

    private static bool IsTransient(HttpStatusCode statusCode)
    {
        return statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;
    }
}

internal sealed record AiCategorizationRequest(
    string Title,
    string Description,
    IReadOnlyList<AiCategoryItem> Categories);

internal sealed record AiCategoryItem(
    string Id,
    string Name,
    string? Description);

internal sealed record AiCategorizationResponse(
    string SuggestedCategoryId,
    string SuggestedCategoryName,
    double Confidence,
    string Reasoning);

internal sealed record AiPriorityRequest(
    string Title,
    string Description,
    string CategoryName,
    int SimilarReportCount,
    int DaysOpen);

internal sealed record AiPriorityResponse(
    double PriorityScore,
    string SuggestedPriority,
    string Reasoning);

internal sealed record AiSimilarityRequest(
    string Title,
    string Description,
    IReadOnlyList<AiSimilarityCandidate> ExistingReports);

internal sealed record AiSimilarityCandidate(
    string Id,
    string Title,
    string Description,
    DateTime CreatedAt);

internal sealed record AiSimilarityResponse(
    IReadOnlyList<AiSimilarityMatch> SimilarReports);

internal sealed record AiSimilarityMatch(
    string ReportId,
    double SimilarityScore,
    string Reasoning);
