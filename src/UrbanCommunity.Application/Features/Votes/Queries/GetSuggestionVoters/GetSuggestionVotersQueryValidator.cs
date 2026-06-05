using FluentValidation;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoters;

public sealed class GetSuggestionVotersQueryValidator : AbstractValidator<GetSuggestionVotersQuery>
{
    public GetSuggestionVotersQueryValidator()
    {
        RuleFor(x => x.SuggestionId).NotEmpty();
    }
}
