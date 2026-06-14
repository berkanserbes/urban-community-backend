namespace UrbanCommunity.Application.Interfaces;

public interface IReportCategorizationService
{
    Task<ReportCategorizationSuggestion?> SuggestCategoryAsync(
        string title,
        string description,
        IReadOnlyList<ReportCategoryCandidate> categories,
        CancellationToken cancellationToken = default);
}

public sealed record ReportCategoryCandidate(
    Guid Id,
    string Name,
    string? Description);

public sealed record ReportCategorizationSuggestion(
    Guid CategoryId,
    string CategoryName,
    double Confidence,
    string Reasoning);
