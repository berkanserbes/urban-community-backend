using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Interfaces;

public interface IReportPriorityService
{
    Task<ReportPrioritySuggestion?> SuggestPriorityAsync(
        string title,
        string description,
        string categoryName,
        int similarReportCount,
        int daysOpen,
        CancellationToken cancellationToken = default);
}

public sealed record ReportPrioritySuggestion(
    ReportPriority Priority,
    double Score,
    string Reasoning);
