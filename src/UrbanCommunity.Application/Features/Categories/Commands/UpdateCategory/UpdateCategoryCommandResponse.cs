namespace UrbanCommunity.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommandResponse(
    Guid Id,
    string Name,
    string? Description,
    string? IconUrl);
