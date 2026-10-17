using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostComments;

public sealed record GetCommunityPostCommentsQuery(Guid CommunityPostId, Guid RequestingUserId, bool IsAdmin = false)
    : PaginationRequest, IRequest<Result<PagedResult<GetCommunityPostCommentsQueryResponse>>>;
