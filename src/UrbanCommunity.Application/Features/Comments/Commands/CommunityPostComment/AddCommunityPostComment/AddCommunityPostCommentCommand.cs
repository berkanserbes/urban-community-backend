using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.AddCommunityPostComment;

public sealed record AddCommunityPostCommentCommand(Guid CommunityPostId, Guid UserId, string Content) : IRequest<Result<AddCommunityPostCommentCommandResponse>>;