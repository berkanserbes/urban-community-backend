using FluentValidation;

namespace UrbanCommunity.Application.Features.Suggestions.Queries.GetAllowedSuggestionStatusTransitions;

public sealed class GetAllowedSuggestionStatusTransitionsQueryValidator
    : AbstractValidator<GetAllowedSuggestionStatusTransitionsQuery>
{
    public GetAllowedSuggestionStatusTransitionsQueryValidator()
    {
        RuleFor(query => query.SuggestionId).NotEmpty();
        RuleFor(query => query.RequestingUserId).NotEmpty();
    }
}
