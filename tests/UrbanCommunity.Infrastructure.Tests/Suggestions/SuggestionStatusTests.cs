using UrbanCommunity.Domain.Common;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Suggestions;

public sealed class SuggestionStatusTests
{
    [Theory]
    [InlineData(SuggestionStatus.Active, SuggestionStatus.Reviewing)]
    [InlineData(SuggestionStatus.Reviewing, SuggestionStatus.Active)]
    [InlineData(SuggestionStatus.Reviewing, SuggestionStatus.Implementation)]
    [InlineData(SuggestionStatus.Reviewing, SuggestionStatus.Closed)]
    [InlineData(SuggestionStatus.Implementation, SuggestionStatus.Reviewing)]
    [InlineData(SuggestionStatus.Implementation, SuggestionStatus.Closed)]
    public void ChangeStatus_AllowsConfiguredTransitions(
        SuggestionStatus current,
        SuggestionStatus next)
    {
        var suggestion = new Suggestion { Status = current };

        suggestion.ChangeStatus(next);

        Assert.Equal(next, suggestion.Status);
    }

    [Theory]
    [InlineData(SuggestionStatus.Active, SuggestionStatus.Implementation)]
    [InlineData(SuggestionStatus.Active, SuggestionStatus.Closed)]
    [InlineData(SuggestionStatus.Implementation, SuggestionStatus.Active)]
    [InlineData(SuggestionStatus.Closed, SuggestionStatus.Active)]
    [InlineData(SuggestionStatus.Closed, SuggestionStatus.Reviewing)]
    public void ChangeStatus_RejectsUnconfiguredTransitions(
        SuggestionStatus current,
        SuggestionStatus next)
    {
        var suggestion = new Suggestion { Status = current };

        Assert.Throws<DomainException>(() => suggestion.ChangeStatus(next));
    }

    [Theory]
    [InlineData(SuggestionStatus.Active, true, true)]
    [InlineData(SuggestionStatus.Reviewing, true, true)]
    [InlineData(SuggestionStatus.Implementation, true, true)]
    [InlineData(SuggestionStatus.Closed, true, false)]
    public void InteractionRules_DependOnStatus(
        SuggestionStatus status,
        bool allowsVotes,
        bool allowsComments)
    {
        var suggestion = new Suggestion { Status = status };

        Assert.Equal(allowsVotes, suggestion.AllowsVotes());
        Assert.Equal(allowsComments, suggestion.AllowsCommentChanges());
    }
}
