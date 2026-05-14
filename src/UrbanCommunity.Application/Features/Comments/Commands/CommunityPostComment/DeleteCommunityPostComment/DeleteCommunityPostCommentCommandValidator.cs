using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.DeleteCommunityPostComment;

public sealed class DeleteCommunityPostCommentCommandValidator : AbstractValidator<DeleteCommunityPostCommentCommand>
{
    public DeleteCommunityPostCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty();
        RuleFor(x => x.RequestingUserId).NotEmpty();
    }
}
