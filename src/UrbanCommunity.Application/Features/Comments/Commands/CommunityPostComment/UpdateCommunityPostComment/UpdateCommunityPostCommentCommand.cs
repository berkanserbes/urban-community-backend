using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.UpdateCommunityPostComment;

public sealed record UpdateCommunityPostCommentCommand(
    Guid CommentId,
    Guid RequestingUserId,
    string Content) : IRequest<Result<UpdateCommunityPostCommentCommandResponse>>;
