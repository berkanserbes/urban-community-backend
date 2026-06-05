using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;

public sealed class GetSuggestionsQueryValidator : AbstractValidator<GetSuggestionsQuery>
{
    public GetSuggestionsQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Geçersiz öneri durumu.")
            .When(x => x.Status.HasValue);
    }
}
