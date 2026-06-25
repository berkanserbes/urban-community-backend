using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestionOfficialResponse;

public sealed class UpdateSuggestionOfficialResponseCommandValidator
    : AbstractValidator<UpdateSuggestionOfficialResponseCommand>
{
    public UpdateSuggestionOfficialResponseCommandValidator()
    {
        RuleFor(x => x.SuggestionId)
            .NotEmpty().WithMessage("Oneri kimligi gereklidir.");

        RuleFor(x => x.RequestingUserId)
            .NotEmpty().WithMessage("Kullanici kimligi belirlenemedi.");

        RuleFor(x => x.OfficialResponse)
            .Must(value => value is null || value.Trim().Length <= 2000)
            .WithMessage("Resmi cevap en fazla 2000 karakter olabilir.");
    }
}
