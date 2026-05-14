using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoteSummary;

public sealed record GetSuggestionVoteSummaryQuery(Guid SuggestionId, Guid? UserId = null)
    : IRequest<Result<GetSuggestionVoteSummaryQueryResponse>>;