using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;

public sealed class ChangeSuggestionStatusCommandValidator : AbstractValidator<ChangeSuggestionStatusCommand>
{
    public ChangeSuggestionStatusCommandValidator()
    {
        RuleFor(x => x.SuggestionId)
            .NotEmpty().WithMessage("Öneri kimliği gereklidir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği belirlenemedi.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Geçersiz öneri durumu.");
    }
}
