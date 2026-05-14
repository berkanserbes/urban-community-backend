using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.DeleteCommunityPostComment;

public sealed record DeleteCommunityPostCommentCommand(Guid CommentId, Guid RequestingUserId, bool IsAdmin = false) : IRequest<Result<DeleteCommunityPostCommentCommandResponse>>;
