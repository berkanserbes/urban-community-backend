using FluentValidation;

namespace UrbanCommunity.Application.Features.Votes.Commands.SetSuggestionVote;

public sealed class SetSuggestionVoteCommandValidator : AbstractValidator<SetSuggestionVoteCommand>
{
    public SetSuggestionVoteCommandValidator()
    {
        RuleFor(x => x.SuggestionId)
            .NotEmpty().WithMessage("Öneri kimliği gereklidir.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("Kullanıcı kimliği belirlenemedi. Lütfen tekrar giriş yapın.");
    }
}
