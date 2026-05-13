namespace UrbanCommunity.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommandResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl);
