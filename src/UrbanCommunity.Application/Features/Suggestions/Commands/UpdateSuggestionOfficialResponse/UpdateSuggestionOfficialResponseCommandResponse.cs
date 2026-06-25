namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestionOfficialResponse;

public sealed record UpdateSuggestionOfficialResponseCommandResponse(
    Guid Id,
    string? OfficialResponse,
    DateTime? UpdatedAt,
    string Message);
