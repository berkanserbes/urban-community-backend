namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategoriesWithReportCount;

public sealed record GetCategoriesWithReportCountQueryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl,
    int ReportCount);
