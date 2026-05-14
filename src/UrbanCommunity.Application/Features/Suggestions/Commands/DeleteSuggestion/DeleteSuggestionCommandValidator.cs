using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;

public sealed class DeleteSuggestionCommandValidator : AbstractValidator<DeleteSuggestionCommand>
{
    public DeleteSuggestionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
