using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;

public sealed record ChangeSuggestionStatusCommandResponse(
    Guid Id,
    string Title,
    SuggestionStatus Status,
    string Message);
