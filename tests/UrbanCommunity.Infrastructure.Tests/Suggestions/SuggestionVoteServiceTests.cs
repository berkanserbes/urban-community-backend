using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Suggestions;

public sealed class SuggestionVoteServiceTests
{
    [Fact]
    public async Task SetVote_AddsVoteAndIncrementsScore()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new SuggestionVoteService(context);

        var result = await service.SetVoteAsync(
            data.Suggestion.Id,
            data.User.Id,
            true,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(1, result.UpvoteCount);
        Assert.Equal(0, result.DownvoteCount);
        Assert.Equal(1, result.VoteScore);
    }

    [Fact]
    public async Task SetVote_ChangingDirectionAppliesTwoPointDelta()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new SuggestionVoteService(context);
        await service.SetVoteAsync(
            data.Suggestion.Id,
            data.User.Id,
            true,
            CancellationToken.None);

        var result = await service.SetVoteAsync(
            data.Suggestion.Id,
            data.User.Id,
            false,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.UpvoteCount);
        Assert.Equal(1, result.DownvoteCount);
        Assert.Equal(-1, result.VoteScore);
        Assert.Single(await context.Votes.ToListAsync());
    }

    [Fact]
    public async Task RemoveVote_ReversesVoteScore()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        var service = new SuggestionVoteService(context);
        await service.SetVoteAsync(
            data.Suggestion.Id,
            data.User.Id,
            false,
            CancellationToken.None);

        var result = await service.RemoveVoteAsync(
            data.Suggestion.Id,
            data.User.Id,
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(0, result.VoteScore);
        Assert.Empty(await context.Votes.ToListAsync());
    }

    [Fact]
    public async Task SetVote_RejectsNonActiveSuggestion()
    {
        await using var context = CreateContext();
        var data = await SeedAsync(context);
        data.Suggestion.Status = SuggestionStatus.Reviewing;
        await context.SaveChangesAsync();
        var service = new SuggestionVoteService(context);

        var result = await service.SetVoteAsync(
            data.Suggestion.Id,
            data.User.Id,
            true,
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(await context.Votes.ToListAsync());
        Assert.Equal(0, data.Suggestion.VoteScore);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static async Task<TestData> SeedAsync(ApplicationDbContext context)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Vote",
            LastName = "User",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com",
            IsActive = true
        };
        var suggestion = new Suggestion
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            User = user,
            Title = "Suggestion",
            Content = "Content",
            Status = SuggestionStatus.Active
        };

        context.Users.Add(user);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();
        return new TestData(user, suggestion);
    }

    private sealed record TestData(User User, Suggestion Suggestion);
}
