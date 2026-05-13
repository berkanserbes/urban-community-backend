using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;

public sealed record GetSuggestionsQueryResponse(
    Guid Id,
    string Title,
    string Content,
    SuggestionStatus Status,
    int VoteScore,
    int UpVotes,
    int DownVotes,
    Guid UserId,
    string UserFullName,
    DateTime CreatedAt);
