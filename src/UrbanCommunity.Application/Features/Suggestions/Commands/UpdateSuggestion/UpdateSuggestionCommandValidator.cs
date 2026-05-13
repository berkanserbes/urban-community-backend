using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestion;

public sealed class UpdateSuggestionCommandValidator : AbstractValidator<UpdateSuggestionCommand>
{
    public UpdateSuggestionCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Öneri kimliği gereklidir.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği gereklidir.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz.")
            .MaximumLength(2000).WithMessage("İçerik en fazla 2000 karakter olabilir.");
    }
}
