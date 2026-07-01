using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Features.Users.Commands.DeactivateUser;
using UrbanCommunity.Application.Features.Users.Commands.UpdateUserAccess;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Users;

public sealed class AdminSafetyTests
{
    [Fact]
    public async Task Deactivate_RejectsSelfDeactivation()
    {
        await using var context = CreateContext();
        var admin = CreateAdmin();
        context.Users.Add(admin);
        await context.SaveChangesAsync();
        var handler = new DeactivateUserCommandHandler(context, new NoopRevocationService());

        var result = await handler.Handle(
            new DeactivateUserCommand(admin.Id, admin.Id),
            default);

        Assert.False(result.IsSuccess);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task Deactivate_RejectsLastActiveAdmin()
    {
        await using var context = CreateContext();
        var admin = CreateAdmin();
        context.Users.Add(admin);
        await context.SaveChangesAsync();
        var handler = new DeactivateUserCommandHandler(context, new NoopRevocationService());

        var result = await handler.Handle(
            new DeactivateUserCommand(admin.Id, Guid.NewGuid()),
            default);

        Assert.False(result.IsSuccess);
        Assert.True(admin.IsActive);
    }

    [Fact]
    public async Task UpdateAccess_RejectsOwnRoleChange()
    {
        await using var context = CreateContext();
        var admin = CreateAdmin();
        context.Users.Add(admin);
        await context.SaveChangesAsync();
        var handler = new UpdateUserAccessCommandHandler(
            context,
            new NoopRevocationService());

        var result = await handler.Handle(
            new UpdateUserAccessCommand(
                admin.Id,
                UserRole.Citizen,
                null,
                null,
                null,
                admin.Id),
            default);

        Assert.False(result.IsSuccess);
        Assert.Equal(UserRole.Admin, admin.Role);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static User CreateAdmin()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Test",
            LastName = "Admin",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com"
        };
        user.UpdateAccess(UserRole.Admin);
        return user;
    }

    private sealed class NoopRevocationService : IRefreshTokenRevocationService
    {
        public Task<int> MarkAllActiveAsRevokedAsync(
            Guid userId,
            string reason,
            CancellationToken cancellationToken) => Task.FromResult(0);
    }
}
