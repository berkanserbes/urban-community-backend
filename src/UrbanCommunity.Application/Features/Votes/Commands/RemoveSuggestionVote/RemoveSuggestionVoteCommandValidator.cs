using FluentValidation;

namespace UrbanCommunity.Application.Features.Votes.Commands.RemoveSuggestionVote;

public sealed class RemoveSuggestionVoteCommandValidator : AbstractValidator<RemoveSuggestionVoteCommand>
{
    public RemoveSuggestionVoteCommandValidator()
    {
        RuleFor(x => x.SuggestionId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
