using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Application.Interfaces;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Domain.Enums;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services;

public sealed class AccessTokenSessionValidatorTests
{
    [Fact]
    public async Task IsValidAsync_ReturnsTrue_WhenActiveUserClaimsMatch()
    {
        await using var context = CreateContext();
        var user = CreateUser(UserRole.Citizen);
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var validator = new AccessTokenSessionValidator(context);

        var result = await validator.IsValidAsync(
            new AccessTokenSessionClaims(user.Id, UserRole.Citizen, null, null, null),
            CancellationToken.None);

        Assert.True(result);
    }

    [Theory]
    [InlineData(false, UserRole.Citizen)]
    [InlineData(true, UserRole.Admin)]
    public async Task IsValidAsync_ReturnsFalse_WhenUserIsInactiveOrRoleChanged(
        bool isActive,
        UserRole tokenRole)
    {
        await using var context = CreateContext();
        var user = CreateUser(UserRole.Citizen);
        user.IsActive = isActive;
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var validator = new AccessTokenSessionValidator(context);

        var result = await validator.IsValidAsync(
            new AccessTokenSessionClaims(user.Id, tokenRole, null, null, null),
            CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task IsValidAsync_ReturnsFalse_WhenOfficialAccessClaimsChanged()
    {
        await using var context = CreateContext();
        var departmentId = Guid.NewGuid();
        var user = CreateUser(UserRole.Official);
        user.UpdateAccess(
            UserRole.Official,
            departmentId,
            OfficialJurisdictionLevel.District,
            Guid.NewGuid());
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var validator = new AccessTokenSessionValidator(context);

        var result = await validator.IsValidAsync(
            new AccessTokenSessionClaims(
                user.Id,
                UserRole.Official,
                departmentId,
                OfficialJurisdictionLevel.District,
                Guid.NewGuid()),
            CancellationToken.None);

        Assert.False(result);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static User CreateUser(UserRole role)
    {
        return new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Session",
            LastName = "User",
            UserName = $"{Guid.NewGuid():N}@example.com",
            Email = $"{Guid.NewGuid():N}@example.com",
            Role = role
        };
    }
}
