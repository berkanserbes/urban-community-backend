using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services.Ai;

internal sealed class ReportCategorizationService(
    IAiApiClient aiApiClient,
    ILogger<ReportCategorizationService> logger)
    : IReportCategorizationService
{
    public async Task<ReportCategorizationSuggestion?> SuggestCategoryAsync(
        string title,
        string description,
        IReadOnlyList<ReportCategoryCandidate> categories,
        CancellationToken cancellationToken = default)
    {
        if (categories.Count == 0)
            return null;

        var request = new AiCategorizationRequest(
            title,
            description,
            categories.Select(category => new AiCategoryItem(
                category.Id.ToString(),
                category.Name,
                category.Description)).ToList());

        var response = await aiApiClient.SuggestReportCategoryAsync(
            request,
            cancellationToken);

        if (response is null)
            return null;

        if (!Guid.TryParse(response.SuggestedCategoryId, out var categoryId) ||
            !double.IsFinite(response.Confidence) ||
            response.Confidence is < 0 or > 1 ||
            string.IsNullOrWhiteSpace(response.SuggestedCategoryName) ||
            response.SuggestedCategoryName.Trim().Length > 100 ||
            string.IsNullOrWhiteSpace(response.Reasoning) ||
            response.Reasoning.Trim().Length > 500)
        {
            logger.LogWarning("AI categorization returned an invalid response.");
            return null;
        }

        return new ReportCategorizationSuggestion(
            categoryId,
            response.SuggestedCategoryName.Trim(),
            response.Confidence,
            response.Reasoning.Trim());
    }
}
