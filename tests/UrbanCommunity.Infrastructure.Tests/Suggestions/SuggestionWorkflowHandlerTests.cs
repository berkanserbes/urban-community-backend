using MassTransit;
using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Features.Comments.Commands.SuggestionComment.AddSuggestionComment;
using UrbanCommunity.Application.Features.Suggestions.Commands.ChangeSuggestionStatus;
using UrbanCommunity.Application.Features.Suggestions.Commands.DeleteSuggestion;
using UrbanCommunity.Application.Features.Suggestions.Commands.UpdateSuggestionOfficialResponse;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestionById;
using UrbanCommunity.Application.Features.Suggestions.Queries.GetSuggestions;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Suggestions;

public sealed class SuggestionWorkflowHandlerTests
{
    private static readonly IPublishEndpoint NoopPublisher = new NoopPublishEndpoint();

    private sealed class NoopPublishEndpoint : IPublishEndpoint
    {
        public Task Publish<T>(T message, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(T message, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish<T>(T message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) where T : class => Task.CompletedTask;
        public Task Publish(object message, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, Type messageType, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task Publish(object message, Type messageType, IPipe<PublishContext> pipe, CancellationToken cancellationToken = default) => Task.CompletedTask;
        Task IPublishEndpoint.Publish<T>(object message, CancellationToken cancellationToken) => Task.CompletedTask;
        Task IPublishEndpoint.Publish<T>(object message, IPipe<PublishContext<T>> pipe, CancellationToken cancellationToken) => Task.CompletedTask;
        Task IPublishEndpoint.Publish<T>(object message, IPipe<PublishContext> pipe, CancellationToken cancellationToken) => Task.CompletedTask;
        public ConnectHandle ConnectPublishObserver(IPublishObserver observer) => new NoopConnectHandle();

        private sealed class NoopConnectHandle : ConnectHandle
        {
            public void Dispose() { }
            public void Disconnect() { }
        }
    }
    [Fact]
    public async Task ChangeStatus_AllowsActiveOfficialFromDatabase()
    {
        await using var context = CreateContext();
        var actor = CreateUser(UserRole.Official);
        var suggestion = CreateSuggestion(Guid.NewGuid());
        context.Users.Add(actor);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new ChangeSuggestionStatusCommandHandler(context, NoopPublisher);
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

        var handler = new ChangeSuggestionStatusCommandHandler(context, NoopPublisher);

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

    [Fact]
    public async Task UpdateOfficialResponse_AllowsOfficialAndTrimsResponse()
    {
        await using var context = CreateContext();
        var actor = CreateUser(UserRole.Official);
        var suggestion = CreateSuggestion(Guid.NewGuid());
        suggestion.Status = SuggestionStatus.Closed;
        context.Users.Add(actor);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new UpdateSuggestionOfficialResponseCommandHandler(context, NoopPublisher);
        var result = await handler.Handle(
            new UpdateSuggestionOfficialResponseCommand(
                suggestion.Id,
                actor.Id,
                "  Official response  "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Official response", suggestion.OfficialResponse);
        Assert.Equal("Official response", result.Data?.OfficialResponse);
    }

    [Fact]
    public async Task UpdateOfficialResponse_RejectsCitizenEvenWhenEndpointCommandIsInvokedDirectly()
    {
        await using var context = CreateContext();
        var actor = CreateUser(UserRole.Citizen);
        var suggestion = CreateSuggestion(Guid.NewGuid());
        context.Users.Add(actor);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new UpdateSuggestionOfficialResponseCommandHandler(context, NoopPublisher);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(
            new UpdateSuggestionOfficialResponseCommand(
                suggestion.Id,
                actor.Id,
                "Official response"),
            CancellationToken.None));
    }

    [Fact]
    public async Task UpdateOfficialResponse_ClearsWhitespaceResponse()
    {
        await using var context = CreateContext();
        var actor = CreateUser(UserRole.Admin);
        var suggestion = CreateSuggestion(Guid.NewGuid());
        suggestion.OfficialResponse = "Existing response";
        context.Users.Add(actor);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new UpdateSuggestionOfficialResponseCommandHandler(context, NoopPublisher);
        var result = await handler.Handle(
            new UpdateSuggestionOfficialResponseCommand(
                suggestion.Id,
                actor.Id,
                "   "),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Null(suggestion.OfficialResponse);
        Assert.Null(result.Data?.OfficialResponse);
    }

    [Fact]
    public async Task GetSuggestionById_ReturnsOfficialResponse()
    {
        await using var context = CreateContext();
        var owner = CreateUser(UserRole.Citizen);
        var suggestion = CreateSuggestion(owner.Id);
        suggestion.OfficialResponse = "Official response";
        context.Users.Add(owner);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new GetSuggestionByIdQueryHandler(context);
        var result = await handler.Handle(new GetSuggestionByIdQuery(suggestion.Id), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("Official response", result.Data?.OfficialResponse);
    }

    [Fact]
    public async Task GetSuggestions_ReturnsOfficialResponse()
    {
        await using var context = CreateContext();
        var owner = CreateUser(UserRole.Citizen);
        var suggestion = CreateSuggestion(owner.Id);
        suggestion.OfficialResponse = "Official response";
        context.Users.Add(owner);
        context.Suggestions.Add(suggestion);
        await context.SaveChangesAsync();

        var handler = new GetSuggestionsQueryHandler(context);
        var result = await handler.Handle(new GetSuggestionsQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Items);
        Assert.Equal("Official response", result.Data!.Items[0].OfficialResponse);
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
