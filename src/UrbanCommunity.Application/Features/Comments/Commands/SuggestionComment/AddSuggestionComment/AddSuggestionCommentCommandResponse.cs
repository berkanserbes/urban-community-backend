namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;

public sealed record AddSuggestionCommentCommandResponse(
    Guid Id,
    Guid UserId,
    string Content,
    DateTime CreatedAt);
