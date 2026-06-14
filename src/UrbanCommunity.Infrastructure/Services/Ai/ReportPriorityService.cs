using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Infrastructure.Services.Ai;

internal sealed class ReportPriorityService(
    IAiApiClient aiApiClient,
    ILogger<ReportPriorityService> logger)
    : IReportPriorityService
{
    public async Task<ReportPrioritySuggestion?> SuggestPriorityAsync(
        string title,
        string description,
        string categoryName,
        int similarReportCount,
        int daysOpen,
        CancellationToken cancellationToken = default)
    {
        var response = await aiApiClient.SuggestReportPriorityAsync(
            new AiPriorityRequest(
                title,
                description,
                categoryName,
                similarReportCount,
                daysOpen),
            cancellationToken);

        if (response is null)
            return null;

        if (!double.IsFinite(response.PriorityScore) ||
            response.PriorityScore is < 0 or > 1 ||
            !Enum.TryParse<ReportPriority>(
                response.SuggestedPriority,
                ignoreCase: true,
                out var priority) ||
            priority != GetPriorityForScore(response.PriorityScore) ||
            string.IsNullOrWhiteSpace(response.Reasoning) ||
            response.Reasoning.Trim().Length > 500)
        {
            logger.LogWarning("AI priority scoring returned an invalid response.");
            return null;
        }

        return new ReportPrioritySuggestion(
            priority,
            response.PriorityScore,
            response.Reasoning.Trim());
    }

    internal static ReportPriority GetPriorityForScore(double score)
    {
        if (score <= 0.25)
            return ReportPriority.Low;
        if (score <= 0.50)
            return ReportPriority.Medium;
        if (score <= 0.75)
            return ReportPriority.High;
        return ReportPriority.Urgent;
    }
}
