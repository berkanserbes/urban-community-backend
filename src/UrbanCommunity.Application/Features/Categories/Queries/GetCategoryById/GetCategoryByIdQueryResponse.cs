namespace UrbanCommunity.Application.Features.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQueryResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl);
