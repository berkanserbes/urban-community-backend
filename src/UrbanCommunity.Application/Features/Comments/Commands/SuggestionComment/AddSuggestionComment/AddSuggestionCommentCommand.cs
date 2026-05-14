using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;

public sealed record AddSuggestionCommentCommand(Guid SuggestionId, Guid UserId, string Content) : IRequest<Result<AddSuggestionCommentCommandResponse>>;