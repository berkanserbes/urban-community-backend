using FluentValidation;

namespace UrbanCommunity.Application.Features.Comments.Commands.CommunityPostComment.AddCommunityPostComment;

public sealed class AddCommunityPostCommentCommandValidator : AbstractValidator<AddCommunityPostCommentCommand>
{
    public AddCommunityPostCommentCommandValidator()
    {
        RuleFor(x => x.CommunityPostId).NotEmpty().WithMessage("Gönderi kimliği gereklidir.");
        RuleFor(x => x.UserId).NotEmpty().WithMessage("Kullanıcı kimliği belirlenemedi.");
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Yorum içeriği boş olamaz.")
            .MaximumLength(1000).WithMessage("Yorum en fazla 1000 karakter olabilir.");
    }
}
