using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;

public sealed class AddSuggestionCommentCommandValidator : AbstractValidator<AddSuggestionCommentCommand>
{
    public AddSuggestionCommentCommandValidator()
    {
        RuleFor(x => x.SuggestionId).NotEmpty().WithMessage("Öneri kimliği gereklidir.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirlenemedi.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Yorum içeriği boş olamaz.")
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
    }
}
