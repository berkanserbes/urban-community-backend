namespace UrbanCommunity.Application.Features.Comments.Queries.CommunityPostComment.GetCommunityPostComments;

public sealed record GetCommunityPostCommentsQueryResponse(
    Guid Id,
    Guid UserId,
    string AuthorName,
    string Content,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
