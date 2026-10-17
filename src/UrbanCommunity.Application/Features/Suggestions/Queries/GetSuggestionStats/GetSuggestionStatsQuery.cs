using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionStats;

public sealed record GetSuggestionStatsQuery : IRequest<Result<GetSuggestionStatsQueryResponse>>;
