using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;

public sealed class GetSuggestionByIdQueryValidator : AbstractValidator<GetSuggestionByIdQuery>
{
    public GetSuggestionByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Öneri kimliği gereklidir.");
    }
}
