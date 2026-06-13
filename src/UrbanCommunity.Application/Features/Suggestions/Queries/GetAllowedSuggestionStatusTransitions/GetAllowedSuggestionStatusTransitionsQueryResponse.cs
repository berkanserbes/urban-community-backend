using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetAllowedSuggestionStatusTransitions;

public sealed record GetAllowedSuggestionStatusTransitionsQueryResponse(
    Guid SuggestionId,
    SuggestionStatus CurrentStatus,
    IReadOnlyList<SuggestionStatus> AllowedNextStatuses);
