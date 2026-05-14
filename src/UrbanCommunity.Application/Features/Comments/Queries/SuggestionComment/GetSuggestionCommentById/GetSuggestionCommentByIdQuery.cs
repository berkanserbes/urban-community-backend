using MediatR;
using UrbanCommunity.Application.Common.Models;

namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionCommentById;

public sealed record GetSuggestionCommentByIdQuery(Guid CommentId)
    : IRequest<Result<GetSuggestionCommentByIdQueryResponse>>;
