namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.AddCommunityPostComment;

public sealed record AddCommunityPostCommentCommandResponse(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAt);
