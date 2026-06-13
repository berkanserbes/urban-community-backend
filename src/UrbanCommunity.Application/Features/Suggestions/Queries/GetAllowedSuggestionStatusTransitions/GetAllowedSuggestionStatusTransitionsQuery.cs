using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetAllowedSuggestionStatusTransitions;

public sealed record GetAllowedSuggestionStatusTransitionsQuery(
    Guid SuggestionId,
    Guid RequestingUserId)
    : IRequest<Result<GetAllowedSuggestionStatusTransitionsQueryResponse>>;
