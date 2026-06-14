using Microsoft.EntityFrameworkCore;
using UrbanCommunity.Domain.Entities;
using UrbanCommunity.Infrastructure.Persistence;
using UrbanCommunity.Infrastructure.Services;
using Xunit;

namespace UrbanCommunity.Infrastructure.Tests.Services;

public sealed class RefreshTokenRevocationServiceTests
{
    [Fact]
    public async Task MarkAllActiveAsRevokedAsync_RevokesOnlyActiveTokensForUser()
    {
        await using var context = CreateContext();
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var activeToken = CreateToken(userId, DateTime.UtcNow.AddHours(1));
        var expiredToken = CreateToken(userId, DateTime.UtcNow.AddHours(-1));
        var alreadyRevokedToken = CreateToken(userId, DateTime.UtcNow.AddHours(1));
        alreadyRevokedToken.RevokedAt = DateTime.UtcNow.AddMinutes(-5);
        alreadyRevokedToken.RevokedReason = "Previously revoked";
        var otherUserToken = CreateToken(otherUserId, DateTime.UtcNow.AddHours(1));

        context.UserRefreshTokens.AddRange(
            activeToken,
            expiredToken,
            alreadyRevokedToken,
            otherUserToken);
        await context.SaveChangesAsync();

        var service = new RefreshTokenRevocationService(context);

        var revokedCount = await service.MarkAllActiveAsRevokedAsync(
            userId,
            "User deactivated",
            CancellationToken.None);
        await context.SaveChangesAsync();

        Assert.Equal(1, revokedCount);
        Assert.NotNull(activeToken.RevokedAt);
        Assert.Equal("User deactivated", activeToken.RevokedReason);
        Assert.Null(expiredToken.RevokedAt);
        Assert.Equal("Previously revoked", alreadyRevokedToken.RevokedReason);
        Assert.Null(otherUserToken.RevokedAt);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    private static UserRefreshToken CreateToken(Guid userId, DateTime expiresAt)
    {
        return new UserRefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = Guid.NewGuid().ToString("N"),
            ExpiresAt = expiresAt
        };
    }
}
