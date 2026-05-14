using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed record GetSuggestionVotersQuery(Guid SuggestionId)
    : IRequest<Result<GetSuggestionVotersQueryResponse>>;
