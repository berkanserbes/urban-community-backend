namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.UpdateSuggestionComment;

public sealed record UpdateSuggestionCommentCommandResponse(
    Guid Id,
    string Content,
    DateTime? UpdatedAt);
