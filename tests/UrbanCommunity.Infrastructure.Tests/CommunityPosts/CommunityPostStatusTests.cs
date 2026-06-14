using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Common.Exceptions;
using UrbanCommunity.Application.Features.CommunityPosts.Commands.ChangeCommunityPostStatus;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.CommunityPosts;

public sealed class CommunityPostStatusTests
{
    [Fact]
    public async Task ChangeStatus_AllowsOwner()
    {
        await using var context = CreateContext();
        var post = CreatePost();
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var handler = new ChangeCommunityPostStatusCommandHandler(context);
        var result = await handler.Handle(
            new ChangeCommunityPostStatusCommand(
                post.Id,
                post.UserId,
                false,
                CommunityPostStatus.Reserved),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(CommunityPostStatus.Reserved, post.Status);
    }

    [Fact]
    public async Task ChangeStatus_RejectsNonOwnerNonAdmin()
    {
        await using var context = CreateContext();
        var post = CreatePost();
        context.CommunityPosts.Add(post);
        await context.SaveChangesAsync();

        var handler = new ChangeCommunityPostStatusCommandHandler(context);

        await Assert.ThrowsAsync<ForbiddenAccessException>(() => handler.Handle(
            new ChangeCommunityPostStatusCommand(
                post.Id,
                Guid.NewGuid(),
                false,
                CommunityPostStatus.Reserved),
            CancellationToken.None));
    }

    [Theory]
    [InlineData(CommunityPostStatus.Active, true)]
    [InlineData(CommunityPostStatus.Reserved, true)]
    [InlineData(CommunityPostStatus.Resolved, false)]
    [InlineData(CommunityPostStatus.Expired, false)]
    [InlineData(CommunityPostStatus.Archived, false)]
    public void AllowsCommentChanges_DependsOnStatus(
        CommunityPostStatus status,
        bool expected)
    {
        var post = CreatePost();
        post.Status = status;

        Assert.Equal(expected, post.AllowsCommentChanges());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static CommunityPost CreatePost() => new()
    {
        Id = Guid.NewGuid(),
        UserId = Guid.NewGuid(),
        Type = PostType.HelpRequested,
        Title = "Post",
        Content = "Content",
        Scope = CommunityPostScope.Neighborhood,
        NeighborhoodId = Guid.NewGuid()
    };
}
