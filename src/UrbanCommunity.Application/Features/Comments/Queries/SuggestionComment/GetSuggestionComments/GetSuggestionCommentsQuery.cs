using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionComments;

public sealed record GetSuggestionCommentsQuery(Guid SuggestionId) : PaginationRequest, IRequest<Result<PagedResult<GetSuggestionCommentsQueryResponse>>>;
