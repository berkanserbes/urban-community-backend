using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;
using UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;
using UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Suggestions;

public sealed class SuggestionWorkflowHandlerTests
{
    [Fact]
    public async Task ChangeStatus_AllowsActiveOfficialFromDatabase()
    {
        await using var context = CreateContext();
        var actor = CreateUser(UserRole.Official);
        var suggestion = CreateSuggestion(Guid.NewGuid());
        context.Users.Add(actor);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new ChangeSuggestionStatusCommandHandler(context);
        var result = await handler.Handle(
            new ChangeSuggestionStatusCommand(
                suggestion.Id,
                actor.Id,
                SuggestionStatus.Reviewing),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SuggestionStatus.Reviewing, suggestion.Status);
    }

    [Fact]
    public async Task ChangeStatus_RejectsCitizenEvenWhenEndpointCommandIsInvokedDirectly()
    {
        await using var context = CreateContext();
        var actor = CreateUser(UserRole.Citizen);
        var suggestion = CreateSuggestion(Guid.NewGuid());
        context.Users.Add(actor);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new ChangeSuggestionStatusCommandHandler(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(
            new ChangeSuggestionStatusCommand(
                suggestion.Id,
                actor.Id,
                SuggestionStatus.Reviewing),
            CancellationToken.None));
    }

    [Fact]
    public async Task Delete_RejectsOwnerWhenSuggestionIsNotActive()
    {
        await using var context = CreateContext();
        var ownerId = Guid.NewGuid();
        var suggestion = CreateSuggestion(ownerId);
        suggestion.Status = SuggestionStatus.Reviewing;
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new DeleteSuggestionCommandHandler(context);
        var result = await handler.Handle(
            new DeleteSuggestionCommand(suggestion.Id, ownerId),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.NotNull(await context.Suggestions.FindAsync(suggestion.Id));
    }

    [Fact]
    public async Task AddComment_RejectsClosedSuggestion()
    {
        await using var context = CreateContext();
        var user = CreateUser(UserRole.Citizen);
        var suggestion = CreateSuggestion(user.Id);
        suggestion.Status = SuggestionStatus.Closed;
        context.Users.Add(user);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new AddSuggestionCommentCommandHandler(context);
        var result = await handler.Handle(
            new AddSuggestionCommentCommand(
                suggestion.Id,
                user.Id,
                "Comment"),
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Empty(await context.SuggestionComments.ToListAsync());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static User CreateUser(UserRole role) => new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Workflow",
        LastName = "User",
        UserName = $"{Guid.NewGuid():N}@example.com",
        Email = $"{Guid.NewGuid():N}@example.com",
        Role = role,
        IsActive = true
    };

    private static Suggestion CreateSuggestion(Guid ownerId) => new()
    {
        Id = Guid.NewGuid(),
        UserId = ownerId,
        Title = "Suggestion",
        Content = "Content",
        Status = SuggestionStatus.Active
    };
}
