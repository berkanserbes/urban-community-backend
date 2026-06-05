using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostCommentById;

public sealed record GetCommunityPostCommentByIdQuery(Guid CommentId, Guid RequestingUserId, bool IsAdmin = false)
    : IRequest<Result<GetCommunityPostCommentByIdQueryResponse>>;
