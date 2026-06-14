using Microsoft.Extensions.Logging;
using UrbanCommunity.Application.Interfaces;

namespace UrbanCommunity.Infrastructure.Services.Ai;

internal sealed class ReportSimilarityService(
    IAiApiClient aiApiClient,
    ILogger<ReportSimilarityService> logger)
    : IReportSimilarityService
{
    public async Task<IReadOnlyList<ReportSimilaritySuggestion>?> FindSimilarReportsAsync(
        string title,
        string description,
        IReadOnlyList<ReportSimilarityCandidate> candidates,
        CancellationToken cancellationToken = default)
    {
        if (candidates.Count == 0)
            return [];

        var candidateIds = candidates.Select(candidate => candidate.Id).ToHashSet();
        var response = await aiApiClient.FindSimilarReportsAsync(
            new AiSimilarityRequest(
                title,
                description,
                candidates.Select(candidate => new AiSimilarityCandidate(
                    candidate.Id.ToString(),
                    candidate.Title,
                    candidate.Description,
                    candidate.CreatedAt)).ToList()),
            cancellationToken);

        if (response?.SimilarReports is null)
            return null;

        var suggestions = new List<ReportSimilaritySuggestion>();
        var seenIds = new HashSet<Guid>();

        foreach (var match in response.SimilarReports)
        {
            if (!Guid.TryParse(match.ReportId, out var reportId) ||
                !candidateIds.Contains(reportId) ||
                !seenIds.Add(reportId) ||
                !double.IsFinite(match.SimilarityScore) ||
                match.SimilarityScore is < 0 or > 1 ||
                string.IsNullOrWhiteSpace(match.Reasoning) ||
                match.Reasoning.Trim().Length > 500)
            {
                logger.LogWarning("AI similarity detection returned an invalid response.");
                return null;
            }

            suggestions.Add(new ReportSimilaritySuggestion(
                reportId,
                match.SimilarityScore,
                match.Reasoning.Trim()));
        }

        return suggestions
            .OrderByDescending(suggestion => suggestion.SimilarityScore)
            .ToList();
    }
}
