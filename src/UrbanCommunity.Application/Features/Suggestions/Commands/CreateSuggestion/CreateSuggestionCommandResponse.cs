namespace UrbanCommunity.Application.Features.Suggestions.Commands.CreateSuggestion;

public sealed record CreateSuggestionCommandResponse(
    Guid Id,
    string Title,
    string Status,
    DateTime CreatedAt);
