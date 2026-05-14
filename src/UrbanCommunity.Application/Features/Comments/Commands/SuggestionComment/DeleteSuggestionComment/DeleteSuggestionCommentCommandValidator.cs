using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.DeleteSuggestionComment;

public sealed class DeleteSuggestionCommentCommandValidator : AbstractValidator<DeleteSuggestionCommentCommand>
{
    public DeleteSuggestionCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
