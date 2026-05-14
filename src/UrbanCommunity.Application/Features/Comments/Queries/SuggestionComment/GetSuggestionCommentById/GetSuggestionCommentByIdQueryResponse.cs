namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionCommentById;

public sealed record GetSuggestionCommentByIdQueryResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
