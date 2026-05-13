namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestion;

public sealed record UpdateSuggestionCommandResponse(
    Guid Id,
    string Title,
    string Content,
    DateTime? UpdatedAt);
