namespace UrbanCommunity.Application.Interfaces;

public interface IReportSimilarityService
{
    Task<IReadOnlyList<ReportSimilaritySuggestion>?> FindSimilarReportsAsync(
        string title,
        string description,
        IReadOnlyList<ReportSimilarityCandidate> candidates,
        CancellationToken cancellationToken = default);
}

public sealed record ReportSimilarityCandidate(
    Guid Id,
    string Title,
    string Description,
    DateTime CreatedAt,
    bool IsExactContentMatch = false);

public sealed record ReportSimilaritySuggestion(
    Guid ReportId,
    double SimilarityScore,
    string Reasoning);
