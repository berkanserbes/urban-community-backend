namespace UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostCommentById;

public sealed record GetCommunityPostCommentByIdQueryResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
