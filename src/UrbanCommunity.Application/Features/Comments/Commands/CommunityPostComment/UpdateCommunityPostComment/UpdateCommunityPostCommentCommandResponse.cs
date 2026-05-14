namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.UpdateCommunityPostComment;

public sealed record UpdateCommunityPostCommentCommandResponse(
    Guid Id,
    string Content,
    DateTime? UpdatedAt);
