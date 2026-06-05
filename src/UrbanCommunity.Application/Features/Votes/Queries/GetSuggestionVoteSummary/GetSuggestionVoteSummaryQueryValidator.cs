using FluentValidation;

namespace UrbanCommunity.Application.Features.Votes.Queries.GetSuggestionVoteSummary;

public sealed class GetSuggestionVoteSummaryQueryValidator : AbstractValidator<GetSuggestionVoteSummaryQuery>
{
    public GetSuggestionVoteSummaryQueryValidator()
    {
        RuleFor(x => x.SuggestionId).NotEmpty();
    }
}
