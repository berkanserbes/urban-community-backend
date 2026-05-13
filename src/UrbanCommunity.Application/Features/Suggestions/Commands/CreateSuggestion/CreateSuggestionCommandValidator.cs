using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.CreateSuggestion;

public sealed class CreateSuggestionCommandValidator : AbstractValidator<CreateSuggestionCommand>
{
    public CreateSuggestionCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Başlık boş olamaz.")
            .MaximumLength(200).WithMessage("Başlık en fazla 200 karakter olabilir.");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("İçerik boş olamaz.")
            .MaximumLength(2000).WithMessage("İçerik en fazla 2000 karakter olabilir.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği gereklidir.");
    }
}
