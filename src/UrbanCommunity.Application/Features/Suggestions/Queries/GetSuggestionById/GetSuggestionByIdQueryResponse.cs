using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;

public sealed record GetSuggestionByIdQueryResponse(
    Guid Id,
    string Title,
    string Content,
    string? OfficialResponse,
    SuggestionStatus Status,
    int VoteScore,
    Guid UserId,
    string UserFullName,
    int UpVotes,
    int DownVotes,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
