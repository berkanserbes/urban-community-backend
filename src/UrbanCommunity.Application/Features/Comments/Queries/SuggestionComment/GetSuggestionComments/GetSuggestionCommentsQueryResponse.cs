namespace UrbanCommunity.Application.Features.Comments.Queries.SuggestionComment.GetSuggestionComments;

public sealed record GetSuggestionCommentsQueryResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
