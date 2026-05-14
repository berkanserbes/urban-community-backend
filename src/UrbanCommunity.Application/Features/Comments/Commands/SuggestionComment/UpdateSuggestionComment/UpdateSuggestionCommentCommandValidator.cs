using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.UpdateSuggestionComment;

public sealed class UpdateSuggestionCommentCommandValidator : AbstractValidator<UpdateSuggestionCommentCommand>
{
    public UpdateSuggestionCommentCommandValidator()
    {
        RuleFor(x => x.CommentId).NotEmpty().WithMessage("Yorum kimliği gereklidir.");
        RuleFor(x => x.RequestingUserId).NotEmpty().WithMessage("Kullanıcı kimliği belirlenemedi.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Yorum içeriği boş olamaz.")
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
    }
}
