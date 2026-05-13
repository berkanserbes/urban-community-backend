using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;

public sealed record GetSuggestionByIdQuery(Guid Id) : IRequest<Result<GetSuggestionByIdQueryResponse>>;
