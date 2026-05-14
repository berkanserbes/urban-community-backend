using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.UpdateSuggestionComment;

public sealed record UpdateSuggestionCommentCommand(
    Guid CommentId,
    Guid RequestingUserId,
    string Content) : IRequest<Result<UpdateSuggestionCommentCommandResponse>>;
