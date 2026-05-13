namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQueryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl);
