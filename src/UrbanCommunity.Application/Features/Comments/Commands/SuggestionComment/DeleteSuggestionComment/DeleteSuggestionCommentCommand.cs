using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.DeleteSuggestionComment;

public sealed record DeleteSuggestionCommentCommand(Guid CommentId, Guid RequestingUserId, bool IsAdmin = false) : IRequest<Result<DeleteSuggestionCommentCommandResponse>>;
