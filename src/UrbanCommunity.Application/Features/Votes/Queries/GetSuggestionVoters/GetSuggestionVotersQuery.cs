using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed record GetSuggestionVotersQuery(Guid SuggestionId, bool? IsUpvote = null)
    : PaginationRequest, IRequest<Result<PagedResult<VoterItem>>>;
