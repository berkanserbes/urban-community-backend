using MediatR;
using UrbanCommunity.Application.Common.Models;
using UrbanCommunity.Domain.Enums;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;

public sealed record GetSuggestionsQuery(
    SuggestionStatus? Status = null,
    Guid? UserId = null) : IRequest<Result<IReadOnlyList<GetSuggestionsQueryResponse>>>;
